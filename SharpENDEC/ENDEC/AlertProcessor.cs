using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static List<SharpDataItem> SharpAlertQueue = new List<SharpDataItem>();

        public static void AlertProcessor()
        {
            while (true)
            {
                if (SharpAlertQueue.Count != 0)
                {
                    SharpDataItem alert;
                    lock (SharpAlertQueue)
                    {
                        alert = SharpAlertQueue[0];
                        SharpAlertQueue.Remove(alert);
                    }
                    ProcessAlertItem(alert);
                }
                Thread.Sleep(100);
            }
        }

        public static void ProcessAlertItem(SharpDataItem relayItem)
        {
            bool IsUI = Settings.Default.WirelessAlertMode;
            foreach (Match match in Regex.Matches(relayItem.Data, @"<valueName>([^<]+)</valueName>\s*<value>([^<]+)</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                if (match.Groups[1].Value == "layer:SOREM:2.0:WirelessText")
                {
                    if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                    {
                        IsUI = true;
                        break;
                    }
                }

                //ConsoleExt.WriteLine($"valueName: {match.Groups[1].Value}");
                //ConsoleExt.WriteLine($"value: {match.Groups[2].Value}");
            }

            Match sentMatch = Regex.Match(relayItem.Data, @"<sent>\s*(.*?)\s*</sent>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match statusMatch = Regex.Match(relayItem.Data, @"<status>\s*(.*?)\s*</status>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match messageTypeMatch = Regex.Match(relayItem.Data, @"<msgType>\s*(.*?)\s*</msgType>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection broadcastImmediatelyMatches = Regex.Matches(relayItem.Data, @"<valueName>layer:SOREM:1.0:Broadcast_Immediately</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection urgencyMatches = Regex.Matches(relayItem.Data, @"<urgency>\s*(.*?)\s*</urgency>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection severityMatches = Regex.Matches(relayItem.Data, @"<severity>\s*(.*?)\s*</severity>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            bool final = false;

            for (int i = 0; i < severityMatches.Count; i++)
            {
                if (Check.Config(relayItem.Data, statusMatch.Groups[1].Value, messageTypeMatch.Groups[1].Value, severityMatches[i].Groups[1].Value, urgencyMatches[i].Groups[1].Value, broadcastImmediatelyMatches[i].Groups[1].Value))
                {
                    final = true;
                    break;
                }
            }

            if (!final)
            {
                ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                return;
            }

            MatchCollection infoMatches = Regex.Matches(relayItem.Data, @"<info>\s*(.*?)\s*</info>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            int infoProc = 0;

            foreach (Match infoMatch in infoMatches)
            {
                infoProc++;
                ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GenericProcessingValueOfValue(Settings.Default.CurrentLanguage, infoProc, infoMatches.Count)}");
                string infoEN = $"<info>{infoMatch.Groups[1].Value}</info>";
                string lang = "en";
                if (Regex.Match(infoEN, @"<language>\s*(.*?)\s*</language>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value == "fr-CA")
                {
                    lang = "fr";
                }
                if (lang != "en")
                {
                    continue;
                }

                string Status = statusMatch.Groups[1].Value;
                string MsgType = messageTypeMatch.Groups[1].Value;
                string EventType = Regex.Match(infoEN, @"<event>\s*(.*?)\s*</event>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                string urgency = Regex.Match(infoEN, @"<urgency>\s*(.*?)\s*</urgency>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                string severity = Regex.Match(infoEN, @"<severity>\s*(.*?)\s*</severity>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                //string when = Regex.Match(@"<sent>\s*(.*?)\s*</sent>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                string broadcastImmediately;
                Match broadcastImmediatelyMatch = Regex.Match(infoEN, @"<valueName>layer:SOREM:1.0:Broadcast_Immediately</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (broadcastImmediatelyMatch.Success)
                {
                    broadcastImmediately = broadcastImmediatelyMatch.Groups[1].Value;
                }
                else
                {
                    broadcastImmediately = "No";
                }

                EventDetails EventInfo = GetEventDetails(EventType);

                ConsoleExt.WriteLine(Status, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(MsgType, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine($"{EventType} | {EventInfo.FriendlyName}", ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(urgency, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(severity, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(broadcastImmediately, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(sentMatch.Groups[1].Value, ConsoleColor.DarkGray);

                bool Stop = false;

                if (Check.Config(infoEN, statusMatch.Groups[1].Value, MsgType, severity, urgency, broadcastImmediately))
                {
                    foreach (string EventName in Settings.Default.EnforceEventBlacklist)
                    {
                        if (EventType.ToLower() == EventName.ToLower())
                        {
                            Stop = true;
                        }
                    }

                    if (Stop)
                    {
                        ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertIgnoredDueToBlacklist(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                        continue;
                    }

                    ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GeneratingProductText(Settings.Default.CurrentLanguage)}");

                    Generate gen = new Generate(infoEN, MsgType, sentMatch.Groups[1].Value);

                    var info = gen.BroadcastInfo(lang);

                    //if (true) //(!string.IsNullOrWhiteSpace(info.BroadcastText))
                    {
                        //ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GeneratingProductAudio}");
                        File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", string.Empty);
                        File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", $"{info.BroadcastText}\x20");
                        File.WriteAllText($"{AssemblyDirectory}\\static-text.txt", $"{info.BroadcastText}\x20");

                        ConsoleExt.WriteLine($"[Alert Processor] -> {info.BroadcastText}", ConsoleColor.Magenta);

                        gen.GenerateAudio(info.BroadcastText, lang);

                        if (IsUI)
                        {
                            Color BackColor;
                            Color ForeColor;

                            switch (severity.ToLower())
                            {
                                case "extreme":
                                    BackColor = Color.Red;
                                    ForeColor = Color.Yellow;
                                    break;
                                case "severe":
                                    BackColor = Color.OrangeRed;
                                    ForeColor = Color.Black;
                                    break;
                                case "moderate":
                                    BackColor = Color.Gold;
                                    ForeColor = Color.Black;
                                    break;
                                case "minor":
                                    BackColor = Color.LightGreen;
                                    ForeColor = Color.Black;
                                    break;
                                case "unknown":
                                default:
                                    BackColor = Color.White;
                                    ForeColor = Color.Black;
                                    break;
                            }

                            //Task.Run(() =>
                            //{
                            //    AlertForm af = new AlertForm
                            //    {
                            //        PlayAudio = () => Play($"{AudioDirectory}\\audio.wav"),
                            //        EventBackColor = BackColor,
                            //        EventForeColor = ForeColor,
                            //        EventTextContent = EventInfo.FriendlyName
                            //    };
                            //    af.Show();
                            //}).Wait(30000);

                            Task.Run(() =>
                            {
                                NotifyOverlay no = new NotifyOverlay
                                {
                                    EventShortInfoText = $"{EventInfo.FriendlyName}",
                                    EventLongInfoText = $"{info.BroadcastText}",
                                    EventTypeText = $"{EventInfo.FriendlyName}"
                                };
                                no.ShowDialog();
                            }).Wait(30000);
                        }
                        else
                        {
                            ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.PlayingAudio(Settings.Default.CurrentLanguage)}");
                            ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\in.wav").AudioLength.TotalMilliseconds} millisecond(s) played");
                            if (EventInfo.Severity.Contains("Severe") || EventInfo.Severity.Contains("Extreme"))
                                ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) played.");
                            else
                            {
                                var (FilePlayed, AudioLength) = Play($"{AudioDirectory}\\attn-minor.wav");
                                if (FilePlayed)
                                    ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\attn-minor.wav").AudioLength.TotalMilliseconds} millisecond(s) played.");
                                else
                                    ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) played.");
                            }
                            //ConsoleExt.WriteLine($"[Alert Processor] Attention tone not played because the alert severity is not severe or extreme.");
                            ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\audio.wav").AudioLength.TotalMilliseconds} millisecond(s) played.");
                            ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\out.wav").AudioLength.TotalMilliseconds} millisecond(s) played.");
                        }

                        File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", string.Empty);
                        File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", $"{info.BroadcastText}\x20");
                    }
                    //else
                    //{
                    //    ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GeneratedProductEmpty(Settings.Default.CurrentLanguage)}");
                    //}
                }
                else
                {
                    ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                }
            }
            //ConsoleExt.WriteLine("[Alert Processor] Processed all available entries.", ConsoleColor.DarkGray);
        }
    }
}
