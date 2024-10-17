using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using SharpENDEC.Properties;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static void FileProcessor()
        {
            try
            {
                while (true)
                {
                    string resultFileName = Check.WatchForFiles();
                    ColorLine($"[File Processor] {LanguageStrings.CapturedFromFileWatcher(Settings.Default.CurrentLanguage)} {resultFileName}", ConsoleColor.Cyan);
                    //if (File.Exists($"{AssemblyDirectory}\\relay.xml")) File.Delete($"{AssemblyDirectory}\\relay.xml");
                    WaitForFile($"{FileQueueDirectory}\\{resultFileName}");
                    string newFileName = $"relay_{rnd.Next(100, 999)}{rnd.Next(100, 999)}.xml";
                    File.Move($"{FileQueueDirectory}\\{resultFileName}", $"{AssemblyDirectory}\\{newFileName}");

                    string relayData = File.ReadAllText($"{AssemblyDirectory}\\{newFileName}", Encoding.UTF8);

                    if (relayData.Contains("<sender>NAADS-Heartbeat</sender>"))
                    {
                        ColorLine($"[File Processor] {LanguageStrings.HeartbeatDetected(Settings.Default.CurrentLanguage)}", ConsoleColor.Green);
                        Match referencesMatch = Regex.Match(relayData, @"<references>\s*(.*?)\s*</references>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (referencesMatch.Success)
                        {
                            string references = referencesMatch.Groups[1].Value;
                            //Console.WriteLine(referencesMatch.Groups[0].Value);
                            //Console.WriteLine(referencesMatch.Groups[1].Value);
                            Check.Heartbeat(references);
                        }
                    }
                    else
                    {
                        ColorLine($"[File Processor] {LanguageStrings.AlertDetected(Settings.Default.CurrentLanguage)}", ConsoleColor.Green);
                        File.Move($"{AssemblyDirectory}\\{newFileName}", $"{FileHistoryDirectory}\\{resultFileName}_{rnd.Next(100, 999)}{rnd.Next(100, 999)}");
                        bool IsUI = Settings.Default.WirelessAlertMode;
                        foreach (Match match in Regex.Matches(relayData, @"<valueName>([^<]+)</valueName>\s*<value>([^<]+)</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline))
                        {
                            if (match.Groups[1].Value == "layer:SOREM:2.0:WirelessText")
                            {
                                if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                                {
                                    IsUI = true;
                                    break;
                                }
                            }

                            //Console.WriteLine($"valueName: {match.Groups[1].Value}");
                            //Console.WriteLine($"value: {match.Groups[2].Value}");
                        }

                        Match sentMatch = Regex.Match(relayData, @"<sent>\s*(.*?)\s*</sent>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match statusMatch = Regex.Match(relayData, @"<status>\s*(.*?)\s*</status>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Match messageTypeMatch = Regex.Match(relayData, @"<msgType>\s*(.*?)\s*</msgType>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        MatchCollection broadcastImmediatelyMatches = Regex.Matches(relayData, @"<valueName>layer:SOREM:1.0:Broadcast_Immediately</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        MatchCollection urgencyMatches = Regex.Matches(relayData, @"<urgency>\s*(.*?)\s*</urgency>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        MatchCollection severityMatches = Regex.Matches(relayData, @"<severity>\s*(.*?)\s*</severity>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        bool final = false;

                        for (int i = 0; i < severityMatches.Count; i++)
                        {
                            if (Check.Config(relayData, statusMatch.Groups[1].Value, messageTypeMatch.Groups[1].Value, severityMatches[i].Groups[1].Value, urgencyMatches[i].Groups[1].Value, broadcastImmediatelyMatches[i].Groups[1].Value))
                            {
                                final = true;
                                break;
                            }
                        }

                        if (!final)
                        {
                            Console.WriteLine($"[File Processor] {LanguageStrings.FileIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}");
                            continue;
                        }

                        MatchCollection infoMatches = Regex.Matches(relayData, @"<info>\s*(.*?)\s*</info>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        int infoProc = 0;

                        foreach (Match infoMatch in infoMatches)
                        {
                            infoProc++;
                            Console.WriteLine($"[File Processor] Processing match {infoProc} of {infoMatches.Count}.");
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

                            ColorLine(Status, ConsoleColor.DarkGray);
                            ColorLine(MsgType, ConsoleColor.DarkGray);
                            ColorLine($"{EventType} | {EventInfo.FriendlyName} | EventInfo.Color", ConsoleColor.DarkGray);
                            ColorLine(urgency, ConsoleColor.DarkGray);
                            ColorLine(severity, ConsoleColor.DarkGray);
                            ColorLine(broadcastImmediately, ConsoleColor.DarkGray);

                            if (Check.Config(infoEN, statusMatch.Groups[1].Value, MsgType, severity, urgency, broadcastImmediately))
                            {
                                Console.WriteLine($"[File Processor] {LanguageStrings.GeneratingProductText(Settings.Default.CurrentLanguage)}");

                                //Console.WriteLine($"0: {messageTypeMatch.Groups[0].Value}");
                                //Console.WriteLine($"1: {messageTypeMatch.Groups[1].Value}");
                                //Console.WriteLine($"2: {messageTypeMatch.Groups[2].Value}");
                                //Console.WriteLine($"3: {messageTypeMatch.Groups[3].Value}");
                                //Console.WriteLine($"4: {messageTypeMatch.Groups[4].Value}");

                                Generate gen = new Generate(infoEN, sentMatch.Groups[1].Value, DateTime.Now.ToString());

                                var info = gen.BroadcastInfo(lang);

                                if (true) //(!string.IsNullOrWhiteSpace(info.BroadcastText))
                                {
                                    //Console.WriteLine($"[File Processor] {LanguageStrings.GeneratingProductAudio}");
                                    File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", string.Empty);
                                    File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", $"{info.BroadcastText}\x20");
                                    File.WriteAllText($"{AssemblyDirectory}\\static-text.txt", $"{info.BroadcastText}\x20");

                                    Console.WriteLine($"[File Processor] -> {info.BroadcastText}");

                                    gen.GenerateAudio(info.BroadcastText, lang);

                                    WaitForFile($"{FileQueueDirectory}\\{resultFileName}");

                                    if (IsUI)
                                    {
                                        Color BackColor;
                                        Color ForeColor;

                                        switch (severity)
                                        {
                                            case "Extreme":
                                                BackColor = Color.Red;
                                                ForeColor = Color.Yellow;
                                                break;
                                            case "Severe":
                                                BackColor = Color.OrangeRed;
                                                ForeColor = Color.Black;
                                                break;
                                            case "Moderate":
                                                BackColor = Color.Gold;
                                                ForeColor = Color.Black;
                                                break;
                                            case "Minor":
                                                BackColor = Color.LightGreen;
                                                ForeColor = Color.Black;
                                                break;
                                            case "Unknown":
                                                BackColor = Color.White;
                                                ForeColor = Color.Black;
                                                break;
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
                                        Console.WriteLine($"[File Processor] {LanguageStrings.PlayingAudio(Settings.Default.CurrentLanguage)}");
                                        Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\in.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                        if (EventInfo.Severity.Contains("Severe") || EventInfo.Severity.Contains("Extreme"))
                                            Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                        else
                                        {
                                            var (FilePlayed, AudioLength) = Play($"{AudioDirectory}\\attn-minor.wav");
                                            if (FilePlayed)
                                                Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\attn-minor.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                            else
                                                Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                        }
                                        //Console.WriteLine($"[File Processor] Attention tone not played because the alert severity is not severe or extreme.");
                                        Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\audio.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                        Console.WriteLine($"[File Processor] Played {Play($"{AudioDirectory}\\out.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                    }

                                    File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", string.Empty);
                                    File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", $"{info.BroadcastText}\x20");
                                }
                                else
                                {
                                    Console.WriteLine($"[File Processor] {LanguageStrings.GeneratedProductEmpty(Settings.Default.CurrentLanguage)}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[File Processor] {LanguageStrings.FileIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}");
                            }
                        }
                    }

                    Console.WriteLine($"[File Processor] {LanguageStrings.LastDataReceived(Settings.Default.CurrentLanguage)} {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine($"{LanguageStrings.ThreadShutdown(Settings.Default.CurrentLanguage, $"File Processor")}");
            }
        }

        public static class Check
        {
            public static bool Config(string InfoX, string Status, string MsgType,
                string Severity, string Urgency, string BroadcastImmediately)
            {
                bool Final = false;

                // Status:
                // Test
                // Actual

                switch (Status.ToLower())
                {
                    case "test":
                        if (!Settings.Default.statusTest) return false;
                        break;
                    case "actual":
                        if (!Settings.Default.statusActual) return false;
                        break;
                    default:
                        break;
                }

                // MsgType:
                // Alert
                // Update
                // Cancel

                if (BroadcastImmediately.ToLower().Contains("yes"))
                {
                    Final = true;
                }
                else
                {
                    try
                    {
                        bool var1; // Severity
                        bool var2; // Urgency
                        bool var3; // MsgType

                        switch (Severity.ToLower())
                        {
                            case "extreme":
                                var1 = Settings.Default.severityExtreme;
                                break;
                            case "severe":
                                var1 = Settings.Default.severitySevere;
                                break;
                            case "moderate":
                                var1 = Settings.Default.severityModerate;
                                break;
                            case "minor":
                                var1 = Settings.Default.severityMinor;
                                break;
                            case "unknown":
                                var1 = Settings.Default.severityUnknown;
                                break;
                            default:
                                var1 = Settings.Default.severityUnknown;
                                break;
                        }

                        switch (Urgency.ToLower())
                        {
                            case "immediate":
                                var2 = Settings.Default.urgencyImmediate;
                                break;
                            case "expected":
                                var2 = Settings.Default.urgencyExpected;
                                break;
                            case "future":
                                var2 = Settings.Default.urgencyFuture;
                                break;
                            case "past":
                                var2 = Settings.Default.urgencyPast;
                                break;
                            default:
                                var2 = false;
                                break;
                        }

                        switch (MsgType.ToLower())
                        {
                            case "alert":
                                var3 = Settings.Default.messageTypeAlert;
                                break;
                            case "update":
                                var3 = Settings.Default.messageTypeUpdate;
                                break;
                            case "cancel":
                                var3 = Settings.Default.messageTypeCancel;
                                break;
                            case "test":
                                var3 = Settings.Default.messageTypeTest;
                                break;
                            default:
                                var3 = false;
                                break;

                        }

                        if (var1 && var2 && var3)
                        {
                            Final = true;
                        }
                    }
                    catch
                    {
                        Final = false;
                    }
                }

                if (Final)
                {
                    if (Settings.Default.AllowedLocations_Geocodes.Count == 0)
                    {

                    }
                    else
                    {
                        try
                        {
                            MatchCollection matches = Regex.Matches(InfoX,
                                @"<geocode>\s*<valueName>profile:CAP-CP:Location:0.3</valueName>\s*<value>\s*(.*?)\s*</value>",
                                RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            bool GeoMatch = false;
                            foreach (Match match in matches)
                            {
                                string geocode = match.Groups[1].Value;
                                if (Settings.Default.AllowedLocations_Geocodes.Contains(geocode))
                                {
                                    GeoMatch = true;
                                    break;
                                }
                            }
                            if (!GeoMatch)
                            {
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            ColorLine($"[File Processor] {ex.Message}", ConsoleColor.Red);
                            return false;
                        }
                    }
                }
                return Final;
            }

            public static DateTime LastHeartbeat = DateTime.Now;

            private static readonly HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            public static void Heartbeat(string References)
            {
                ColorLine($"[Heartbeat] {LanguageStrings.DownloadingFiles(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkYellow);
                string[] RefList = References.Split(' ');
                int FilesMatched = 0;
                foreach (string i in RefList)
                {
                    string filename = string.Empty;
                    string[] k = i.Split(',');
                    string sentDTFull = k[2].Replace("-", "_").Replace(":", "_").Replace("+", "p");
                    string sentDT = sentDTFull.Substring(0, 10).Replace("_", "-");
                    filename += sentDTFull;
                    filename += "I" + k[1].Replace("-", "_").Replace(":", "_");
                    //string filenameShort = sentDTFull.Replace("_", "-");

                    string Dom1 = "capcp1.naad-adna.pelmorex.com";
                    string Dom2 = "capcp2.naad-adna.pelmorex.com";

                    string outputFile = FileQueueDirectory + $"\\{filename}.{rnd.Next(100, 999)}{rnd.Next(100, 999)}.xml";

                    if (File.Exists(FileHistoryDirectory + $"\\{filename}.xml"))
                    {
                        FilesMatched++;
                        continue;
                    }

                    ColorLine($"[Heartbeat] Downloading: {filename}.xml...", ConsoleColor.Yellow);
                    string url1 = $"http://{Dom1}/{sentDT}/{filename}.xml";
                    string url2 = $"http://{Dom2}/{sentDT}/{filename}.xml";

                    try
                    {
                        ColorLine($"-> {url1}", ConsoleColor.Yellow);
                        Task<string> xml = client.GetStringAsync(url1);
                        xml.Wait();
                        if (!File.Exists(outputFile)) File.WriteAllText(outputFile, xml.Result);
                        else ColorLine("[Heartbeat] There is no need to download an existing file.", ConsoleColor.Yellow);
                        xml.Dispose();
                    }
                    catch (Exception e1)
                    {
                        try
                        {
                            ColorLine($"[Heartbeat] Stage 1 failed: {e1.Message}", ConsoleColor.Red);
                            ColorLine($"[Heartbeat] Downloading: {filename}.xml from backup...", ConsoleColor.Yellow);
                            ColorLine($"-> {url2}", ConsoleColor.Yellow);
                            Task<string> xml = client.GetStringAsync(url2);
                            xml.Wait();
                            if (!File.Exists(outputFile)) File.WriteAllText(outputFile, xml.Result);
                            else ColorLine("[Heartbeat] There is no need to download an existing file.", ConsoleColor.Yellow);
                            xml.Dispose();
                        }
                        catch (Exception e2)
                        {
                            ColorLine($"[Heartbeat] Stage 2 failed: {e2.Message}", ConsoleColor.Red);
                            ColorLine($"[Heartbeat] Failed to download the file.", ConsoleColor.Red);
                        }
                    }
                }
                if (FilesMatched != 0) ColorLine($"[Heartbeat] {LanguageStrings.FilesIgnoredDueToMatchingPairs(Settings.Default.CurrentLanguage, FilesMatched)}", ConsoleColor.Blue);
                LastHeartbeat = DateTime.Now;
            }

            public static string WatchForFiles()
            {
                Console.WriteLine($"[File Processor] {LanguageStrings.WatchingFiles(Settings.Default.CurrentLanguage)}");

                while (true)
                {
                    if (!Directory.Exists(FileQueueDirectory)) Directory.CreateDirectory(FileQueueDirectory);
                    if (!Directory.Exists(FileHistoryDirectory)) Directory.CreateDirectory(FileHistoryDirectory);
                    string[] folderQueue = Directory.GetFiles(FileQueueDirectory);
                    foreach (string file in folderQueue)
                    {
                        string fileName = Path.GetFileName(file);
                        if (File.Exists(Path.Combine(FileHistoryDirectory, fileName)))
                        {
                            ColorLine($"[File Processor] {LanguageStrings.FileIgnoredDueToMatchingPair(Settings.Default.CurrentLanguage)}", ConsoleColor.Blue);
                            File.Delete(file);
                        }
                        else
                        {
                            return fileName;
                        }
                    }
                    Thread.Sleep(100);
                }
            }
        }

        public class Generate
        {
            private readonly string InfoX;
            private readonly string MsgType;
            private readonly string Sent;

            public Generate(string InfoXML, string MsgTypeZ, string SentDate)
            {
                InfoX = InfoXML;
                MsgType = MsgTypeZ;
                Sent = SentDate;
            }

            public (string BroadcastText, bool) BroadcastInfo(string lang)
            {
                string BroadcastText = "";

                Match match = Regex.Match(InfoX, @"<valueName>layer:SOREM:1.0:Broadcast_Text</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    BroadcastText = match.Groups[1].Value.Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ").Trim();
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

                    DateTime sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

                    string SentFormatted = lang == "fr" ? $"{sentDate:HH}'{sentDate:h}'{sentDate:mm zzz}." : $"{sentDate:HH:mm z}, {sentDate:MMMM dd}, {sentDate:yyyy}.";

                    string EventType;
                    try
                    {
                        EventType = Regex.Match(InfoX, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Name</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        EventType = Regex.Match(InfoX, @"<event>\s*(.*?)\s*</event>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                        EventType = lang == "fr" ? $"alerte {EventType}" : $"{EventType} alert";
                    }

                    string Coverage;
                    try
                    {
                        Coverage = Regex.Match(InfoX, @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Coverage</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                        Coverage = lang == "fr" ? $"en {Coverage} pour:" : $"in {Coverage} for:";
                    }
                    catch (Exception)
                    {
                        Coverage = lang == "fr" ? "pour:" : "for:";
                    }

                    string[] areaDescMatches = Regex.Matches(InfoX, @"<areaDesc>\s*(.*?)\s*</areaDesc>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline)
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .ToArray();

                    string AreaDesc = string.Join(", ", areaDescMatches) + ".";

                    string SenderName;

                    try
                    {
                        SenderName = Regex.Match(InfoX, @"<senderName>\s*(.*?)\s*</senderName>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        SenderName = "an alert issuer";
                    }

                    string Description;

                    try
                    {
                        Description = Regex.Match(InfoX, @"<description>\s*(.*?)\s*</description>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        if (!Description.EndsWith(".")) Description += ".";
                    }
                    catch (Exception)
                    {
                        Description = "";
                    }

                    string Instruction;

                    try
                    {
                        Instruction = Regex.Match(InfoX, @"<instruction>\s*(.*?)\s*</instruction>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        if (!Instruction.EndsWith(".")) Instruction += ".";
                    }
                    catch (Exception)
                    {
                        Instruction = "";
                    }

                    BroadcastText = lang == "fr" ?
                        $"À {SentFormatted} {SenderName} a {MsgPrefix} une {EventType} {Coverage} {AreaDesc} {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim() :
                        $"At {SentFormatted} {SenderName} has {MsgPrefix} a {EventType} {Coverage} {AreaDesc} {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim();
                }

                if (BroadcastText.EndsWith("\x20.")) BroadcastText = BroadcastText.TrimEnd('\x20', '.');
                if (!BroadcastText.EndsWith(".") || !BroadcastText.EndsWith("!")) BroadcastText += ".";

                //if (Debugger.IsAttached) BroadcastText += "\x20| Debugging in progress";

                return (BroadcastText, true);
            }

            public bool GetAudio(string audioLink, string output, int decodeType)
            {
                if (decodeType == 1)
                {
                    Console.WriteLine("Decoding audio from Base64.");
                    try
                    {
                        byte[] audioData = Convert.FromBase64String(audioLink);
                        File.WriteAllBytes(output, audioData);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Decoder failed: {ex.Message}");
                        return false;
                    }
                }
                else if (decodeType == 0)
                {
                    Console.WriteLine("Downloading audio.");
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
                        Console.WriteLine($"Downloader failed: {ex.Message}");
                    }
                    return false;
                }
                else
                {
                    Console.WriteLine("Invalid DecodeType specified.");
                    return false;
                }
            }

            private readonly SpeechSynthesizer engine = new SpeechSynthesizer();

            public void GenerateAudio(string text, string lang)
            {
                try
                {
                    Match Resource = Regex.Match(InfoX, @"<resourceDesc>\s*(.*?)\s*</resourceDesc>\s*(.*?)\s*</resource>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

                    Console.WriteLine(audioLink);
                    Thread.Sleep(5000);

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
                            Console.WriteLine("Post processing failed.");
                            File.Move(audioFile, $"{AudioDirectory}\\audio.wav");
                        }

                        //string ffmpegCmd = $"{AssemblyDirectory}\\ffmpeg.exe -y -i {audioFile} -filter:a volume=2.5 {AssemblyDirectory.Replace("\\", "/")}/Audio/audio.wav";
                        //Process p = Process.Start("cmd.exe", $"/c {ffmpegCmd}");
                        //p.WaitForExit(12000);
                        //if (p.ExitCode != 0 || !File.Exists($"{AssemblyDirectory}\\Audio\\audio.wav"))
                        //{
                        //    Console.WriteLine("Post processing failed. Please make sure ffmpeg is in the program folder.");
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
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(lang);
                    foreach (var voice in engine.GetInstalledVoices())
                    {
                        if (voice.VoiceInfo.Name.Contains(Settings.Default.SpeechVoice))
                        {
                            ColorLine(voice.VoiceInfo.Name, ConsoleColor.Magenta);
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
