using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Speech.Synthesis;
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
            // check if file is even valid
            if (!relayItem.Data.StartsWith("<?xml"))
            {
                ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertInvalid(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                return;
            }
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
                try
                {
                    if (Check.Config(relayItem.Data, statusMatch.Groups[1].Value, messageTypeMatch.Groups[1].Value, severityMatches[i].Groups[1].Value, urgencyMatches[i].Groups[1].Value, broadcastImmediatelyMatches[i].Groups[1].Value))
                    {
                        final = true;
                        break;
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    ConsoleExt.WriteLineErr(e.Message);
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
                Match effectiveMatch = Regex.Match(relayItem.Data, @"<effective>\s*(.*?)\s*</effective>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match expiryMatch = Regex.Match(relayItem.Data, @"<expires>\s*(.*?)\s*</expires>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
                ConsoleExt.WriteLine(effectiveMatch.Groups[1].Value, ConsoleColor.DarkGray);
                ConsoleExt.WriteLine(expiryMatch.Groups[1].Value, ConsoleColor.DarkGray);

                if (DateTime.Parse(expiryMatch.Groups[1].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) <= DateTime.Now)
                {
                    ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertIgnoredDueToExpiry(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                    continue;
                }

                bool Stop = false;

                if (Check.Config(infoEN, statusMatch.Groups[1].Value, MsgType, severity, urgency, broadcastImmediately))
                {
                    //foreach (var handler in handlers)
                    //{
                    //    var handlerInstance = (ISharpPlugin)Activator.CreateInstance(handler);
                    //    if (!handlerInstance.RelayAlert(infoEN, statusMatch.Groups[1].Value, MsgType, severity, urgency, broadcastImmediately))
                    //        Stop = true;
                    //}

                    foreach (string EventName in Settings.Default.EnforceEventBlacklist)
                    {
                        if (EventType.ToLower() == EventName.ToLower())
                        {
                            Stop = true;
                            break;
                        }
                    }

                    if (Stop)
                    {
                        ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.AlertIgnoredDueToBlacklist(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                        //foreach (var handler in handlers)
                        //{
                        //    var handlerInstance = (ISharpPlugin)Activator.CreateInstance(handler);
                        //    handlerInstance.AlertBlacklisted();
                        //}
                        continue;
                    }

                    ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GeneratingProductText(Settings.Default.CurrentLanguage)}");

                    Generate gen = new Generate(infoEN, MsgType, sentMatch.Groups[1].Value);
                    var info = gen.BroadcastInfo(lang);

                    //if (true) //(!string.IsNullOrWhiteSpace(info.BroadcastText))
                    {
                        //ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.GeneratingProductAudio}");
                        File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", string.Empty);
                        File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", $"{info}\x20");
                        File.WriteAllText($"{AssemblyDirectory}\\static-text.txt", $"{info}\x20");

                        ConsoleExt.WriteLine($"[Alert Processor] -> {info}", ConsoleColor.Magenta);

                        //foreach (var handler in handlers)
                        //{
                        //    var handlerInstance = (ISharpPlugin)Activator.CreateInstance(handler);
                        //    handlerInstance.ProvideBroadcastText(info.BroadcastText);
                        //}

                        gen.GenerateAudio(info, lang);

                        //foreach (var handler in handlers)
                        //{
                        //    var handlerInstance = (ISharpPlugin)Activator.CreateInstance(handler);
                        //    handlerInstance.AlertRelayingNow(info.BroadcastText);
                        //}

                        //if (IsUI)
                        //{
                        //    Color BackColor;
                        //    Color ForeColor;

                        //    switch (severity.ToLower())
                        //    {
                        //        case "extreme":
                        //            BackColor = Color.Red;
                        //            ForeColor = Color.Yellow;
                        //            break;
                        //        case "severe":
                        //            BackColor = Color.OrangeRed;
                        //            ForeColor = Color.Black;
                        //            break;
                        //        case "moderate":
                        //            BackColor = Color.Gold;
                        //            ForeColor = Color.Black;
                        //            break;
                        //        case "minor":
                        //            BackColor = Color.LightGreen;
                        //            ForeColor = Color.Black;
                        //            break;
                        //        case "unknown":
                        //        default:
                        //            BackColor = Color.White;
                        //            ForeColor = Color.Black;
                        //            break;
                        //    }

                        //    //Task.Run(() =>
                        //    //{
                        //    //    AlertForm af = new AlertForm
                        //    //    {
                        //    //        PlayAudio = () => Play($"{AudioDirectory}\\audio.wav"),
                        //    //        EventBackColor = BackColor,
                        //    //        EventForeColor = ForeColor,
                        //    //        EventTextContent = EventInfo.FriendlyName
                        //    //    };
                        //    //    af.Show();
                        //    //}).Wait(30000);

                        //    Task.Run(() =>
                        //    {
                        //        NotifyOverlay no = new NotifyOverlay
                        //        {
                        //            EventShortInfoText = $"{EventInfo.FriendlyName}",
                        //            EventLongInfoText = $"{info}",
                        //            EventTypeText = $"{EventInfo.FriendlyName}"
                        //        };
                        //        no.ShowDialog();
                        //    }).Wait(30000);
                        //}
                        //else
                        {

                            ConsoleExt.WriteLine($"[Alert Processor] {LanguageStrings.PlayingAudio(Settings.Default.CurrentLanguage)}");
                            ConsoleExt.WriteLine($"[Alert Processor] {Play($"{AudioDirectory}\\in.wav").AudioLength.TotalMilliseconds} millisecond(s) played");
                            if (EventInfo.Severity.ToLower().Contains("severe") || EventInfo.Severity.ToLower().Contains("extreme"))
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
                        File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", $"{info}\x20");
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

        public class Generate
        {
            private readonly string InfoData;
            private readonly string MsgType;
            private readonly string Sent;
            //private readonly string Effective;
            //private readonly string Expires;

            public Generate(string InfoDataZ, string MsgTypeZ, string SentDate)
            {
                InfoData = InfoDataZ;
                MsgType = MsgTypeZ;
                Sent = SentDate;
                //Effective = EffectiveDate;
                //Expires = ExpiryDate;
            }

            public string BroadcastInfo(string lang)
            {
                string BroadcastText = "";

                string SentenceAppendEnd(string value)
                {
                    value = value.Trim();
                    if (value.EndsWith(".") || value.EndsWith("!") || value.EndsWith(",")) return value;
                    else return value += ".";
                }

                string SentenceAppendSpace(string value)
                {
                    value = value.Trim();
                    if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                    else return value += "\x20";
                }

                string SentencePuncuationCorrection(string value)
                {
                    value = value.Trim();
                    while (value.EndsWith("\x20.") || value.EndsWith("\x20,"))
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                    return value = SentenceAppendEnd(value.Substring(0, value.Length - 2));
                }

                string issue, update, cancel;
                if (lang == "fr")
                {
                    issue = "émis";
                    update = "mis à jour";
                    cancel = "annulé";
                }
                else
                {
                    issue = "issued";
                    update = "updated";
                    cancel = "cancelled";
                }

                string MsgPrefix;
                switch (MsgType.ToLower())
                {
                    case "alert":
                        MsgPrefix = issue;
                        break;
                    case "update":
                        MsgPrefix = update;
                        break;
                    case "cancel":
                        MsgPrefix = cancel;
                        break;
                    default:
                        MsgPrefix = "issued";
                        break;
                }

                DateTime sentDate;
                try
                {
                    sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                }
                catch (Exception e)
                {
                    ConsoleExt.WriteLine(e.Message);
                    sentDate = DateTime.Now;
                }

                DateTime effectiveDate;
                try
                {
                    effectiveDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                }
                catch (Exception e)
                {
                    ConsoleExt.WriteLine(e.Message);
                    effectiveDate = DateTime.Now;
                }

                DateTime expiryDate;
                try
                {
                    expiryDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                }
                catch (Exception e)
                {
                    ConsoleExt.WriteLine(e.Message);
                    expiryDate = DateTime.Now.AddHours(1);
                }

                //DateTime sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal).ToUniversalTime();

                string TimeZoneName = "Unknown Time Zone";

                switch (lang)
                {
                    case "fr":
                        //TimeZoneName = OffsetToTimeZoneName(int.Parse(sentISO));
                        TimeZoneName = "Temps Universel Coordonné";
                        break;
                    case "en":
                    default:
                        //TimeZoneName = OffsetToTimeZoneName(int.Parse(sentISO));
                        TimeZoneName = "Coordinated Universal Time";
                        break;
                }

                string SentFormatted = lang == "fr" ? $"{sentDate:HH}'{sentDate:h}'{sentDate:mm} {TimeZoneName}" : $"{sentDate:HH:mm} {TimeZoneName}, {sentDate:MMMM dd}, {sentDate:yyyy}";
                string BeginFormatted = lang == "fr" ? $"{effectiveDate:HH}'{effectiveDate:h}'{effectiveDate:mm} {TimeZoneName}" : $"{effectiveDate:HH:mm} {TimeZoneName}, {effectiveDate:MMMM dd}, {effectiveDate:yyyy}";
                string EndFormatted = lang == "fr" ? $"{expiryDate:HH}'{expiryDate:h}'{expiryDate:mm} {TimeZoneName}" : $"{expiryDate:HH:mm} {TimeZoneName}, {expiryDate:MMMM dd}, {expiryDate:yyyy}";

                // add uppercase to first letter of EventType if possible.
                string EventType;
                try
                {
                    EventType = Regex.Match(InfoData, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Name</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    EventType = Regex.Replace(EventType.ToLower(), @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                    EventType = lang == "fr" ? $"Le type d'événement est {EventType}." : $"Event type is {EventType}.";
                }
                catch (Exception)
                {
                    try
                    {
                        EventType = Regex.Match(InfoData, @"<event>\s*(.*?)\s*</event>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                        EventType = Regex.Replace(EventType.ToLower(), @"(^\w)|(\s\w)", m => m.Value.ToUpper());
                        EventType = lang == "fr" ? $"Le type d'événement est {EventType}." : $"Event type is {EventType}.";
                    }
                    catch (Exception)
                    {
                        EventType = lang == "fr" ? $"Le type d'événement n'a pas été spécifié." : $"Event type is not known.";
                    }
                }

                string Coverage;
                try
                {
                    Coverage = Regex.Match(InfoData, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Coverage</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    Coverage = lang == "fr" ? $"Pour {Coverage} :" : $"For {Coverage}:";
                }
                catch (Exception)
                {
                    Coverage = lang == "fr" ? "Pour :" : "For:";
                }

                string[] areaDescMatches = Regex.Matches(InfoData, @"<areaDesc>\s*(.*?)\s*</areaDesc>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToArray();

                string AreaDesc = string.Join(", ", areaDescMatches) + ".";

                string SenderName;

                try
                {
                    SenderName = Regex.Match(InfoData, @"<senderName>\s*(.*?)\s*</senderName>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                }
                catch (Exception)
                {
                    SenderName = "an unknown issuer";
                }

                string Description;

                try
                {
                    Description = SentenceAppendEnd(Regex.Match(InfoData, @"<description>\s*(.*?)\s*</description>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " "));
                }
                catch (Exception)
                {
                    Description = "";
                }

                string Instruction;

                try
                {
                    Instruction = SentenceAppendEnd(Regex.Match(InfoData, @"<instruction>\s*(.*?)\s*</instruction>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " "));
                }
                catch (Exception)
                {
                    Instruction = "";
                }

                string Effective;

                try
                {
                    Effective = effectiveDate.ToString();
                }
                catch (Exception)
                {
                    Effective = "currently";
                }

                string Expiry;

                try
                {
                    Expiry = expiryDate.ToString();
                }
                catch (Exception)
                {
                    Expiry = "soon";
                }

                // Effective {Effective}, and expiring {Expires}.

                BroadcastText += SentenceAppendSpace(EventType);
                BroadcastText += SentenceAppendSpace(Coverage);
                BroadcastText += SentenceAppendSpace(SentenceAppendEnd(AreaDesc));

                if (BeginFormatted != EndFormatted)
                {
                    switch (lang)
                    {
                        case "fr":
                            BroadcastText += SentenceAppendSpace($"Alerte émise le {SentFormatted}, {MsgPrefix} par {SenderName}.");
                            BroadcastText += SentenceAppendSpace($"Cette alerte prend effet le {BeginFormatted}, et expire le {EndFormatted}.");
                            break;
                        case "en":
                        default:
                            BroadcastText += SentenceAppendSpace($"Alertez {MsgPrefix} sur {SentFormatted}, par {SenderName}.");
                            BroadcastText += SentenceAppendSpace($"This alert takes effect on {BeginFormatted}, and expires on {EndFormatted}.");
                            break;
                    }
                }
                    
                BroadcastText += SentenceAppendSpace(SentenceAppendEnd(Description));
                BroadcastText += SentenceAppendSpace(SentenceAppendEnd(Instruction));
                
                Match match = Regex.Match(InfoData, @"<valueName>layer:SOREM:1.0:Broadcast_Text</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    BroadcastText = match.Groups[1].Value.Replace("\r\n", "\x20").Replace("\n", "\x20").Replace("\x20\x20\x20", "\x20").Replace("\x20\x20", "\x20").Trim();
                }

                BroadcastText = SentencePuncuationCorrection(BroadcastText.Replace("###", string.Empty).Replace("\x20\x20\x20", "\x20").Replace("\x20\x20", "\x20").Trim());
                BroadcastText = SentenceAppendEnd(BroadcastText);

                return BroadcastText;
            }

            public string LegacyBroadcastInfo(string lang)
            {
                string BroadcastText = "";

                string SentenceAppendEnd(string value)
                {
                    value = value.Trim();
                    if (value.EndsWith(".") || value.EndsWith("!") || value.EndsWith(",")) return value;
                    else return value += ".";
                }

                string SentenceAppendSpace(string value)
                {
                    value = value.Trim();
                    if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                    else return value += "\x20";
                }

                string SentencePuncuationCorrection(string value)
                {
                    value = value.Trim();
                    while (value.EndsWith("\x20.") || value.EndsWith("\x20,"))
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                    return value = SentenceAppendEnd(value.Substring(0, value.Length - 2));
                }

                Match match = Regex.Match(InfoData, @"<valueName>layer:SOREM:1.0:Broadcast_Text</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    BroadcastText = match.Groups[1].Value.Replace("\r\n", "\x20").Replace("\n", "\x20").Replace("\x20\x20\x20", "\x20").Replace("\x20\x20", "\x20").Trim();
                }
                else
                {
                    string issue, update, cancel;
                    if (lang == "fr")
                    {
                        issue = "émis";
                        update = "mis à jour";
                        cancel = "annulé";
                    }
                    else
                    {
                        issue = "issued";
                        update = "updated";
                        cancel = "cancelled";
                    }

                    string MsgPrefix;
                    switch (MsgType.ToLower())
                    {
                        case "alert":
                            MsgPrefix = issue;
                            break;
                        case "update":
                            MsgPrefix = update;
                            break;
                        case "cancel":
                            MsgPrefix = cancel;
                            break;
                        default:
                            MsgPrefix = "issued";
                            break;
                    }

                    DateTime sentDate;
                    try
                    {
                        sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    }
                    catch (Exception e)
                    {
                        ConsoleExt.WriteLine(e.Message);
                        sentDate = DateTime.Now;
                    }

                    DateTime effectiveDate;
                    try
                    {
                        effectiveDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    }
                    catch (Exception e)
                    {
                        ConsoleExt.WriteLine(e.Message);
                        effectiveDate = DateTime.Now;
                    }

                    DateTime expiryDate;
                    try
                    {
                        expiryDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    }
                    catch (Exception e)
                    {
                        ConsoleExt.WriteLine(e.Message);
                        expiryDate = DateTime.Now.AddHours(1);
                    }

                    //DateTime sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

                    string TimeZoneName = "Unknown Time Zone";

                    string OffsetToTimeZoneName(int utcOffset)
                    {
                        utcOffset = +1;
                        DateTime now = DateTime.UtcNow;
                        List<string> matchingTimeZones = new List<string>();
                        foreach (TimeZoneInfo tz in TimeZoneInfo.GetSystemTimeZones())
                        {
                            TimeSpan offset = tz.BaseUtcOffset;

                            if (offset.Hours == utcOffset)
                            {
                                ConsoleExt.WriteLine($"{tz.DaylightName} {tz.DaylightName}", ConsoleColor.DarkGray);
                                if (tz.IsDaylightSavingTime(now))
                                {
                                    matchingTimeZones.Add(tz.DaylightName);
                                }
                                else
                                {
                                    matchingTimeZones.Add(tz.StandardName);
                                }
                            }
                        }

                        return matchingTimeZones.Count > 0 ? matchingTimeZones[0] : $"UTC -{utcOffset}";
                    }

                    string sentISO = sentDate.ToString("zz");

                    switch (lang)
                    {
                        case "fr":
                            TimeZoneName = OffsetToTimeZoneName(int.Parse(sentISO));
                            break;
                        case "en":
                        default:
                            TimeZoneName = OffsetToTimeZoneName(int.Parse(sentISO));
                            break;
                    }

                    string SentFormatted = lang == "fr" ? $"{sentDate:HH}'{sentDate:h}'{sentDate:mm} {TimeZoneName}." : $"{sentDate:HH:mm} {TimeZoneName}, {sentDate:MMMM dd}, {sentDate:yyyy}.";
                    string BeginFormatted = lang == "fr" ? $"{effectiveDate:HH}'{effectiveDate:h}'{effectiveDate:mm} {TimeZoneName}." : $"{effectiveDate:HH:mm} {TimeZoneName}, {effectiveDate:MMMM dd}, {effectiveDate:yyyy}.";
                    string EndFormatted = lang == "fr" ? $"{expiryDate:HH}'{expiryDate:h}'{expiryDate:mm} {TimeZoneName}." : $"{expiryDate:HH:mm} {TimeZoneName}, {expiryDate:MMMM dd}, {expiryDate:yyyy}.";

                    string EventType;
                    try
                    {
                        EventType = Regex.Match(InfoData, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Name</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        EventType = Regex.Match(InfoData, @"<event>\s*(.*?)\s*</event>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                        EventType = lang == "fr" ? $"alerte {EventType}" : $"{EventType} alert";
                    }

                    string Coverage;
                    try
                    {
                        Coverage = Regex.Match(InfoData, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Coverage</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                        Coverage = lang == "fr" ? $"en {Coverage} pour:" : $"in {Coverage} for:";
                    }
                    catch (Exception)
                    {
                        Coverage = lang == "fr" ? "pour:" : "for:";
                    }

                    string[] areaDescMatches = Regex.Matches(InfoData, @"<areaDesc>\s*(.*?)\s*</areaDesc>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline)
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .ToArray();

                    string AreaDesc = string.Join(", ", areaDescMatches) + ".";

                    string SenderName;

                    try
                    {
                        SenderName = Regex.Match(InfoData, @"<senderName>\s*(.*?)\s*</senderName>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        SenderName = "an alert issuer";
                    }

                    string Description;

                    try
                    {
                        Description = SentenceAppendEnd(Regex.Match(InfoData, @"<description>\s*(.*?)\s*</description>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " "));
                    }
                    catch (Exception)
                    {
                        Description = "";
                    }

                    string Instruction;

                    try
                    {
                        Instruction = SentenceAppendEnd(Regex.Match(InfoData, @"<instruction>\s*(.*?)\s*</instruction>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " "));
                    }
                    catch (Exception)
                    {
                        Instruction = "";
                    }

                    string Effective;

                    try
                    {
                        Effective = effectiveDate.ToString();
                    }
                    catch (Exception)
                    {
                        Effective = "currently";
                    }

                    string Expiry;

                    try
                    {
                        Expiry = expiryDate.ToString();
                    }
                    catch (Exception)
                    {
                        Expiry = "soon";
                    }

                    // Effective {Effective}, and expiring {Expires}.

                    switch (lang)
                    {
                        case "fr":
                            BroadcastText = SentenceAppendSpace("À");
                            BroadcastText += SentenceAppendSpace(SentFormatted);
                            BroadcastText += SentenceAppendSpace(SenderName);
                            BroadcastText += SentenceAppendSpace("a");
                            BroadcastText += SentenceAppendSpace(MsgPrefix);
                            BroadcastText += SentenceAppendSpace("une");
                            break;
                        case "en":
                        default:
                            BroadcastText = SentenceAppendSpace("At");
                            BroadcastText += SentenceAppendSpace(SentFormatted);
                            BroadcastText += SentenceAppendSpace(SenderName);
                            BroadcastText += SentenceAppendSpace("has");
                            BroadcastText += SentenceAppendSpace(MsgPrefix);
                            BroadcastText += SentenceAppendSpace("a");
                            break;
                    }
                    BroadcastText += SentenceAppendSpace(EventType);
                    BroadcastText += SentenceAppendSpace(Coverage);
                    BroadcastText += SentenceAppendSpace(SentenceAppendEnd(AreaDesc));
                    BroadcastText += SentenceAppendSpace(SentenceAppendEnd(Description));
                    BroadcastText += SentenceAppendSpace(SentenceAppendEnd(Instruction));
                    BroadcastText = SentencePuncuationCorrection(BroadcastText.Replace("###", string.Empty).Replace("\x20\x20\x20", "\x20").Replace("\x20\x20", "\x20").Trim());
                    //BroadcastText = lang == "fr" ?
                    //    $"À {SentFormatted} {SenderName} a {MsgPrefix} une {EventType} {Coverage} {AreaDesc}. {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim() :
                    //    $"At {SentFormatted} {SenderName} has {MsgPrefix} a {EventType} {Coverage} {AreaDesc}. {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim();
                }

                BroadcastText = SentenceAppendEnd(BroadcastText);
                //if (BroadcastText.EndsWith("\x20.")) BroadcastText = BroadcastText.TrimEnd('\x20', '.');
                //if (BroadcastText.EndsWith(".")) BroadcastText = BroadcastText.TrimEnd('.');
                //if (!BroadcastText.EndsWith(".") || !BroadcastText.EndsWith("!")) BroadcastText += ".";

                return BroadcastText;
            }

            public bool GetAudio(string audioLink, string output, int decodeType)
            {
                if (decodeType == 1)
                {
                    ConsoleExt.WriteLine("Decoding audio from Base64.");
                    try
                    {
                        byte[] audioData = Convert.FromBase64String(audioLink);
                        File.WriteAllBytes(output, audioData);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ConsoleExt.WriteLine($"Decoder failed: {ex.Message}");
                        return false;
                    }
                }
                else if (decodeType == 0)
                {
                    ConsoleExt.WriteLine("Downloading audio.");
                    try
                    {
                        using (HttpClient webClient = new HttpClient())
                        {
                            File.WriteAllBytes(output, webClient.GetByteArrayAsync(audioLink).Result);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ConsoleExt.WriteLine($"Downloader failed: {ex.Message}");
                    }
                    return false;
                }
                else
                {
                    ConsoleExt.WriteLine("Invalid DecodeType specified.");
                    return false;
                }
            }

            private readonly SpeechSynthesizer engine = new SpeechSynthesizer();

            public void GenerateAudio(string text, string lang)
            {
                try
                {
                    Match Resource = Regex.Match(InfoData, @"<resourceDesc>\s*(.*?)\s*</resourceDesc>\s*(.*?)\s*</resource>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (!Resource.Success) throw new Exception("Audio field not found. TTS will be generated instead.");
                    string broadcastAudioResource = Resource.Groups[1].Value;
                    string audioLink = string.Empty;
                    string audioType = string.Empty;
                    int decode = -1;

                    if (broadcastAudioResource.Contains("<derefUri>"))
                    {
                        Match Link = Regex.Match(broadcastAudioResource, @"<derefUri>\s*(.*?)\s*</derefUri>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match Type = Regex.Match(broadcastAudioResource, @"<mimeType>\s*(.*?)\s*</mimeType>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        decode = 1;
                        if (Link.Success && Type.Success)
                        {
                            audioLink = Link.Groups[1].Value;
                            audioType = Type.Groups[1].Value;
                        }
                    }
                    else
                    {
                        Match Link = Regex.Match(broadcastAudioResource, @"<uri>\s*(.*?)\s*</uri>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match Type = Regex.Match(broadcastAudioResource, @"<mimeType>\s*(.*?)\s*</mimeType>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        decode = 0;
                        if (Link.Success && Type.Success)
                        {
                            audioLink = Link.Groups[1].Value;
                            audioType = Type.Groups[1].Value;
                        }
                    }

                    ConsoleExt.WriteLine(audioLink);
                    //Thread.Sleep(5000);

                    string audioFile;
                    switch (audioType)
                    {
                        case "audio/mpeg":
                            audioFile = "PreAudio.mp3";
                            break;
                        case "audio/x-ms-wma":
                            audioFile = "PreAudio.wma";
                            break;
                        default:
                            audioFile = "PreAudio.wav";
                            break;
                    }

                    if (File.Exists($"{AudioDirectory}\\{audioFile}")) File.Delete($"{AudioDirectory}\\{audioFile}");
                    if (File.Exists($"{AudioDirectory}\\audio.wav")) File.Delete($"{AudioDirectory}\\audio.wav");

                    if (GetAudio(audioLink, audioFile, decode))
                    {
                        using (var audioFileReader = new AudioFileReader(audioFile))
                        {
                            var volumeSampleProvider = new VolumeSampleProvider(audioFileReader.ToSampleProvider())
                            {
                                Volume = 2.5f,
                            };
                            WaveFileWriter.CreateWaveFile16($"{AudioDirectory}\\audio.wav", volumeSampleProvider);
                        }

                        if (!File.Exists($"{AudioDirectory}\\audio.wav"))
                        {
                            ConsoleExt.WriteLine("Post processing failed.");
                            File.Move(audioFile, $"{AudioDirectory}\\audio.wav");
                        }

                        //string ffmpegCmd = $"{AssemblyDirectory}\\ffmpeg.exe -y -i {audioFile} -filter:a volume=2.5 {AssemblyDirectory.Replace("\\", "/")}/Audio/audio.wav";
                        //Process p = Process.Start("cmd.exe", $"/c {ffmpegCmd}");
                        //p.WaitForExit(12000);
                        //if (p.ExitCode != 0 || !File.Exists($"{AssemblyDirectory}\\Audio\\audio.wav"))
                        //{
                        //    ConsoleExt.WriteLine("Post processing failed. Please make sure ffmpeg is in the program folder.");
                        //    File.Move(audioFile, $"{AssemblyDirectory}\\Audio\\audio.wav");
                        //}
                        //new SoundPlayer($"{AssemblyDirectory}\\Audio\\audio.wav").PlaySync();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception ex)
                {
                    ConsoleExt.WriteLine(ex.Message);

                    //ConsoleExt.WriteLine(lang);
                    foreach (var voice in engine.GetInstalledVoices())
                    {
                        //ConsoleExt.WriteLine(voice.VoiceInfo.Culture.TwoLetterISOLanguageName.ToLower());
                        if (voice.VoiceInfo.Name.Contains(Settings.Default.SpeechVoice) && voice.VoiceInfo.Culture.TwoLetterISOLanguageName.ToLower() == lang)
                        {
                            //ConsoleExt.WriteLine(voice.VoiceInfo.Name, ConsoleColor.Magenta);
                            engine.SelectVoice(voice.VoiceInfo.Name);
                            break;
                        }
                    }

                    text = text.Replace("#", "hashtag\x20");
                    text = text.Replace("*", "star\x20");

                    engine.SetOutputToWaveFile($"{AudioDirectory}\\audio.wav");
                    engine.Speak(text);
                    engine.Dispose();
                }
            }
        }
    }
}
