using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpENDEC
{
    public static class ENDEC
    {
        public static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string AudioDirectory = $"{AssemblyDirectory}\\Audio";
        public static readonly string FileQueueDirectory = $"{AssemblyDirectory}\\FileQueue";
        public static readonly string FileHistoryDirectory = $"{AssemblyDirectory}\\FileHistory";

        public class Battery
        {
            public void Monitor()
            {
                // TODO: Add setting to enable and disable battery monitoring
                while (true)
                {
                    try
                    {
                        PowerStatus powerStatus = SystemInformation.PowerStatus;
                        BatteryChargeStatus chargeStatus = powerStatus.BatteryChargeStatus;

                        if (!(powerStatus.BatteryLifePercent > 0.20))
                        {
                            Console.WriteLine($"[Battery] The battery percentage is currently at {powerStatus.BatteryLifePercent}.");
                        }
                        Console.WriteLine($"Battery Status: {chargeStatus}");

                        if ((chargeStatus & BatteryChargeStatus.Charging) == BatteryChargeStatus.Charging)
                        {
                            Console.WriteLine("The system is currently charging.");
                        }
                        else if (chargeStatus == BatteryChargeStatus.NoSystemBattery)
                        {
                            Console.WriteLine("No battery is installed.");
                        }
                        else
                        {
                            Console.WriteLine("The system is not charging.");
                        }
                    }
                    catch (Exception)
                    {

                    }
                    Thread.Sleep(30000);
                }
            }
        }

        public class Capture
        {
            public bool ShutdownCapture = false;

            private void Receive(string host, int port, string delimiter)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(host, port);
                        NetworkStream stream = client.GetStream();
                        stream.ReadTimeout = 2500;
                        Console.WriteLine($"[{host}:{port}] {SVDictionary.ConnectedToServer(Settings.Default.CurrentLanguage, host, port)}");

                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE
                        //while (true) Check.LastHeartbeat = DateTime.Now;
                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE

                        string dataReceived = string.Empty;

                        List<byte> data = new List<byte>();

                        while (true)
                        {
                            while (!stream.DataAvailable)
                            {
                                try
                                {
                                    stream.WriteByte(0);
                                }
                                catch (IOException)
                                {
                                    return;
                                }
                                Thread.Sleep(500);
                            }
                            data.Clear();
                            DateTime now = DateTime.Now;
                            Console.WriteLine($"[{host}:{port}] {SVDictionary.ProcessingStream(Settings.Default.CurrentLanguage)}");

                            while (stream.DataAvailable)
                            {
                                int bit = stream.ReadByte();
                                if (bit != -1)
                                {
                                    data.Add((byte)bit);
                                    Math.Pow(bit, bit);
                                }
                            }

                            Console.WriteLine($"[{host}:{port}] {SVDictionary.ProcessedStream(Settings.Default.CurrentLanguage, data.Count, now)}");
                            string chunk = Encoding.UTF8.GetString(data.ToArray(), 0, data.Count);

                            dataReceived += chunk;

                            // dataReceived.StartsWith("<?xml version='1.0' encoding='UTF-8' standalone='no'?>")

                            if (chunk.Contains(delimiter))
                            {
                                string capturedSent = Regex.Match(dataReceived, @"<sent>\s*(.*?)\s*</sent>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value.Replace("-", "_").Replace("+", "p").Replace(":", "_");
                                string capturedIdent = Regex.Match(dataReceived, @"<identifier>\s*(.*?)\s*</identifier>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value.Replace("-", "_").Replace("+", "p").Replace(":", "_");
                                string naadsFilename = $"{capturedSent}I{capturedIdent}.xml";
                                Directory.CreateDirectory(FileQueueDirectory);
                                if (!File.Exists($"{FileQueueDirectory}\\{naadsFilename}") && !File.Exists($"{FileHistoryDirectory}\\{naadsFilename}"))
                                {
                                    File.WriteAllText($"{FileQueueDirectory}\\{naadsFilename}", dataReceived, Encoding.UTF8);
                                    Console.WriteLine($"[{host}:{port}] {SVDictionary.FileDownloaded(Settings.Default.CurrentLanguage, FileQueueDirectory, naadsFilename, host)}");
                                }
                                else
                                {
                                    Console.WriteLine($"[{host}:{port}] The file has already been downloaded and/or processed.");
                                }
                                dataReceived = string.Empty;
                            }
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"[Capture] {SVDictionary.HostTimedOut(Settings.Default.CurrentLanguage, host)}");
                    return;
                }
            }

            public bool Main()
            {
                List<Thread> ClientThreads = new List<Thread>();

                foreach (string server in Settings.Default.CanadianServers)
                {
                    Thread thread = new Thread(() => Receive(server, 8080, "</alert>"));
                    thread.Start();
                    ClientThreads.Add(thread);
                }

                try
                {
                    while (true)
                    {
                        for (int i = 0; i < ClientThreads.Count; i++)
                        {
                            if (!ClientThreads[i].IsAlive)
                            {
                                Console.WriteLine($"{SVDictionary.RestartingAfterException(Settings.Default.CurrentLanguage)}");
                                string server = Settings.Default.CanadianServers[i];
                                Thread newThread = new Thread(() => Receive(server, 8080, "</alert>"));
                                newThread.Start();
                                ClientThreads[i] = newThread;
                            }
                        }

                        if (ShutdownCapture)
                        {
                            foreach (var thread in ClientThreads)
                            {
                                try
                                {
                                    thread.Abort();
                                }
                                catch (Exception)
                                {

                                }
                            }
                            ShutdownCapture = false;
                            return true;
                        }

                        Thread.Sleep(1000);
                    }
                }
                catch (Exception)
                {
                    return false;
                }
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
                            ColorLine($"[Relayer] {ex.Message}", ConsoleColor.Red);
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
                ColorLine($"[Heartbeat] {SVDictionary.DownloadingFiles(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkYellow);
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

                    string outputfile = FileQueueDirectory + $"\\{filename}.xml";

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
                        File.WriteAllText(outputfile, xml.Result);

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
                            File.WriteAllText(outputfile, xml.Result);
                            xml.Dispose();
                        }
                        catch (Exception e2)
                        {
                            ColorLine($"[Heartbeat] Stage 2 failed: {e2.Message}", ConsoleColor.Red);
                            ColorLine($"[Heartbeat] Failed to process the file.", ConsoleColor.Red);
                        }
                    }
                }
                ColorLine($"[Heartbeat] {SVDictionary.FilesIgnoredDueToMatchingPairs(Settings.Default.CurrentLanguage, FilesMatched)}", ConsoleColor.Blue);
                LastHeartbeat = DateTime.Now;
            }

            public static string WatchNotify()
            {
                Console.WriteLine($"[Relayer] {SVDictionary.WatchingFiles(Settings.Default.CurrentLanguage)}");

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
                            ColorLine($"[Relayer] {SVDictionary.FileIgnoredDueToMatchingPair(Settings.Default.CurrentLanguage)}", ConsoleColor.Blue);
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

        public static void CheckFolder(string folderPath, bool clear)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            else
            {
                if (clear)
                {
                    ClearFolder(folderPath);
                }
            }
        }

        public static void ClearFolder(string directory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        /// <summary>
        /// This method should only be called when the program is started or restarted.
        /// </summary>
        public static void Init(string VersionID, bool RecoveredFromProblem)
        {
            Console.WriteLine($"{VersionID}\r\n" +
                $"Ported from ApatheticDELL's QuantumENDEC 4.\r\n\r\n" +
                $"Project created by BunnyTub.\r\n" +
                $"Logo created by ApatheticDELL.\r\n" +
                $"Translations may not be 100% accurate due to language deviations.");

            if (RecoveredFromProblem) ColorLine(SVDictionary.RecoveredFromFailure(Settings.Default.CurrentLanguage), ConsoleColor.DarkRed);

            CheckFolder(FileQueueDirectory, RecoveredFromProblem);
            CheckFolder(FileHistoryDirectory, RecoveredFromProblem);
            CheckFolder("Audio", false);

            if (!File.Exists("Audio\\attn.wav"))
            {
                MemoryStream mem = new MemoryStream();
                Resources.attn.CopyTo(mem);
                File.WriteAllBytes("Audio\\attn.wav", mem.ToArray());
                mem.Dispose();
            }

            Thread.Sleep(2500);
        }

        /// <summary>
        /// Presents options for the user to configure.
        /// Should only be run in a single threaded environment.
        /// </summary>
        public static void Config()
        {
            for (int i = 5; i > 0; i--)
            {
                Console.Clear();
                Console.WriteLine($"Press C to configure settings. Press R to reset settings. Continuing otherwise in {i} second(s).");

                Thread.Sleep(1000);

                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.C:
                            bool ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Sound ---");
                                Console.WriteLine($"1. SoundDevice = {Settings.Default.SoundDevice} | Use the specified sound device for audio output");
                                Console.WriteLine($"2. SpeechVoice = {Settings.Default.SpeechVoice} | Use the specified voice for TTS when audio is not provided");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Console.Clear();
                                        for (int n = -1; n < WaveOut.DeviceCount; n++)
                                        {
                                            var caps = WaveOut.GetCapabilities(n);
                                            Console.WriteLine($"{n}: {caps.ProductName}");
                                        }
                                        Console.Write("Enter sound device ID:\x20");
                                        Settings.Default.SoundDevice = int.Parse(Console.ReadLine());
                                        break;
                                    case ConsoleKey.D2:
                                        Console.Clear();
                                        foreach (var voice in new SpeechSynthesizer().GetInstalledVoices()) Console.WriteLine(voice.VoiceInfo.Name);
                                        Console.Write("Enter speech voice name:\x20");
                                        Settings.Default.SpeechVoice = Console.ReadLine();
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Statuses ---");
                                Console.WriteLine($"1. statusTest = {Settings.Default.statusTest} | Relay test alerts");
                                Console.WriteLine($"2. statusActual = {Settings.Default.statusActual} | Relay actual alerts");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.statusTest = !Settings.Default.statusTest;
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.statusActual = !Settings.Default.statusActual;
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Types ---");
                                Console.WriteLine($"1. messageTypeAlert = {Settings.Default.messageTypeAlert} | Relay alert messages");
                                Console.WriteLine($"2. messageTypeUpdate = {Settings.Default.messageTypeUpdate} | Relay update messages");
                                Console.WriteLine($"3. messageTypeCancel = {Settings.Default.messageTypeCancel} | Relay cancel messages");
                                Console.WriteLine($"4. messageTypeTest = {Settings.Default.messageTypeTest} | Relay test messages");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.messageTypeAlert = !Settings.Default.messageTypeAlert;
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.messageTypeUpdate = !Settings.Default.messageTypeUpdate;
                                        break;
                                    case ConsoleKey.D3:
                                        Settings.Default.messageTypeCancel = !Settings.Default.messageTypeCancel;
                                        break;
                                    case ConsoleKey.D4:
                                        Settings.Default.messageTypeTest = !Settings.Default.messageTypeTest;
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Severities ---");
                                Console.WriteLine($"1. severityExtreme = {Settings.Default.severityExtreme} | Relay extreme messages");
                                Console.WriteLine($"2. severitySevere = {Settings.Default.severitySevere} | Relay severe messages");
                                Console.WriteLine($"3. severityModerate = {Settings.Default.severityModerate} | Relay moderate messages");
                                Console.WriteLine($"4. severityMinor = {Settings.Default.severityMinor} | Relay minor messages");
                                Console.WriteLine($"5. severityUnknown = {Settings.Default.severityUnknown} | Relay unknown messages");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.severityExtreme = !Settings.Default.severityExtreme;
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.severitySevere = !Settings.Default.severitySevere;
                                        break;
                                    case ConsoleKey.D3:
                                        Settings.Default.severityModerate = !Settings.Default.severityModerate;
                                        break;
                                    case ConsoleKey.D4:
                                        Settings.Default.severityMinor = !Settings.Default.severityMinor;
                                        break;
                                    case ConsoleKey.D5:
                                        Settings.Default.severityUnknown = !Settings.Default.severityUnknown;
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Urgency ---");
                                Console.WriteLine($"1. urgencyImmediate = {Settings.Default.urgencyImmediate} | Relay immediate urgency messages");
                                Console.WriteLine($"2. urgencyExpected = {Settings.Default.urgencyExpected} | Relay expected urgency messages");
                                Console.WriteLine($"3. urgencyFuture = {Settings.Default.urgencyFuture} | Relay future urgency messages");
                                Console.WriteLine($"4. urgencyPast = {Settings.Default.urgencyPast} | Relay past urgency messages");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.urgencyImmediate = !Settings.Default.urgencyImmediate;
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.urgencyExpected = !Settings.Default.urgencyExpected;
                                        break;
                                    case ConsoleKey.D3:
                                        Settings.Default.urgencyFuture = !Settings.Default.urgencyFuture;
                                        break;
                                    case ConsoleKey.D4:
                                        Settings.Default.urgencyPast = !Settings.Default.urgencyPast;
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Geocodes ---");
                                Console.WriteLine();
                                foreach (string geo in Settings.Default.AllowedLocations_Geocodes) Console.WriteLine(geo);
                                if (Settings.Default.AllowedLocations_Geocodes.Count == 0) Console.WriteLine("All");
                                Console.WriteLine();
                                Console.WriteLine($"1. AllowedLocations_Geocodes | Relay messages with the specified geocodes");
                                Console.WriteLine($"2. Relay messages with any location values");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Console.Clear();
                                        Console.Write("Enter geocode to add:\x20");
                                        Settings.Default.AllowedLocations_Geocodes.Add(Console.ReadLine());
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.AllowedLocations_Geocodes.Clear();
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            ProgressToNextPage = false;

                            while (!ProgressToNextPage)
                            {
                                Console.Clear();
                                Console.WriteLine($"--- Miscellaneous ---");
                                Console.WriteLine($"1. CurrentLanguage = {Settings.Default.CurrentLanguage} | Use the program in a different language");
                                Console.WriteLine();
                                Console.WriteLine($"Press ENTER to finish.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.CurrentLanguage = "en";
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }
                            Console.Clear();
                            Settings.Default.Save();
                            Console.WriteLine("Your preferences have been saved.");
                            Thread.Sleep(2500);
                            Console.Clear();
                            return;
                        case ConsoleKey.R:
                            Console.Clear();
                            Settings.Default.Reset();
                            Console.WriteLine("All preferences have been restored to their default configuration.");
                            Thread.Sleep(2500);
                            Console.Clear();
                            return;
                    }
                }
            }
            Console.Clear();
        }

        public static void ColorLine(string value, ConsoleColor foreground)
        {
            var og = Console.ForegroundColor;
            Console.ForegroundColor = foreground;
            Console.WriteLine(value);
            Console.ForegroundColor = og;
        }

        public static EventDetails GetEventDetails(string eventCode)
        {
            var eventDetailsDictionary = new Dictionary<string, EventDetails>
            {
                { "airQuality", new EventDetails("Air Quality", "Severe or Extreme", "Observed") },
                { "civilEmerg", new EventDetails("Civil Emergency", "Severe or Extreme", "Observed") },
                { "terrorism", new EventDetails("Terrorism", "Severe or Extreme", "Observed") },
                { "animalDang", new EventDetails("Dangerous Animal", "Severe or Extreme", "Observed") },
                { "wildFire", new EventDetails("Wildfire", "Severe or Extreme", "Likely or Observed") },
                { "industryFire", new EventDetails("Industrial Fire", "Severe or Extreme", "Observed") },
                { "urbanFire", new EventDetails("Urban Fire", "Severe or Extreme", "Observed") },
                { "forestFire", new EventDetails("Forest Fire", "Severe or Extreme", "Likely or Observed") },
                { "stormSurge", new EventDetails("Storm Surge", "Severe or Extreme", "Likely or Observed") },
                { "flashFlood", new EventDetails("Flash Flood", "Severe or Extreme", "Likely or Observed") },
                { "damOverflow", new EventDetails("Dam Overflow", "Severe or Extreme", "Likely or Observed") },
                { "earthquake", new EventDetails("Earthquake", "Severe or Extreme", "Likely or Observed") },
                { "magnetStorm", new EventDetails("Magnetic Storm", "Severe or Extreme", "Likely or Observed") },
                { "landslide", new EventDetails("Landslide", "Severe or Extreme", "Likely or Observed") },
                { "meteor", new EventDetails("Meteor", "Severe or Extreme", "Likely or Observed") },
                { "tsunami", new EventDetails("Tsunami", "Severe or Extreme", "Likely or Observed") },
                { "lahar", new EventDetails("Lahar", "Severe or Extreme", "Likely or Observed") },
                { "pyroclasticS", new EventDetails("Pyroclastic Surge", "Severe or Extreme", "Likely or Observed") },
                { "pyroclasticF", new EventDetails("Pyroclastic Flow", "Severe or Extreme", "Likely or Observed") },
                { "volcanicAsh", new EventDetails("Volcanic Ash", "Severe or Extreme", "Likely or Observed") },
                { "chemical", new EventDetails("Chemical", "Severe or Extreme", "Observed") },
                { "biological", new EventDetails("Biological", "Severe or Extreme", "Observed") },
                { "radiological", new EventDetails("Radiological", "Severe or Extreme", "Observed") },
                { "explosives", new EventDetails("Explosives", "Severe or Extreme", "Likely or Observed") },
                { "fallObject", new EventDetails("Falling Object", "Severe or Extreme", "Observed") },
                { "drinkingWate", new EventDetails("Drinking Water", "Severe or Extreme", "Observed") },
                { "amber", new EventDetails("Amber Alert", "Severe or Extreme", "Observed") },
                { "hurricane", new EventDetails("Hurricane", "Severe or Extreme", "Observed") },
                { "thunderstorm", new EventDetails("Thunderstorm", "Severe or Extreme", "Observed") },
                { "tornado", new EventDetails("Tornado", "Severe or Extreme", "Likely or Observed") },
                { "testMessage", new EventDetails("Test Message", "Minor", "Observed") },
                { "911Service", new EventDetails("911 Service", "Severe or Extreme", "Observed") }
            };

            if (eventDetailsDictionary.TryGetValue(eventCode, out EventDetails eventDetails))
            {
                return eventDetails;
            }
            else
            {
                return new EventDetails(eventCode, "Unknown Severity", "Unknown Certainty");
            }
        }

        public class EventDetails
        {
            public string FriendlyName { get; }
            public string Severity { get; }
            public string Certainty { get; }

            public EventDetails(string friendlyName, string severity, string certainty)
            {
                FriendlyName = friendlyName;
                Severity = severity;
                Certainty = certainty;
            }
        }

        private static void WaitForFile(string filePath)
        {
            Task.Run(() =>
            {
                bool fileInUse = true;
                while (fileInUse)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                fileInUse = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{filePath} does not exist yet.");
                            fileInUse = false;
                        }
                    }
                    catch (IOException)
                    {
                        fileInUse = true;
                        Console.WriteLine($"{filePath} is still in use.");
                        Thread.Sleep(500);
                    }
                }
            }).Wait(2500);
        }

        public static void Relay()
        {
            while (true)
            {
                Console.WriteLine($"{SVDictionary.LastDataReceived(Settings.Default.CurrentLanguage)} {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
                string resultFileName = Check.WatchNotify();
                ColorLine($"[Relayer] {SVDictionary.CapturedFromFileWatcher(Settings.Default.CurrentLanguage)} {resultFileName}", ConsoleColor.Cyan);
                if (File.Exists($"{AssemblyDirectory}\\relay.xml")) File.Delete($"{AssemblyDirectory}\\relay.xml");
                WaitForFile($"{FileQueueDirectory}\\{resultFileName}");
                File.Move($"{FileQueueDirectory}\\{resultFileName}", $"{AssemblyDirectory}\\relay.xml");

                string relayXML = File.ReadAllText("relay.xml", Encoding.UTF8);

                if (relayXML.Contains("<sender>NAADS-Heartbeat</sender>"))
                {
                    ColorLine($"[Relayer] {SVDictionary.HeartbeatDetected(Settings.Default.CurrentLanguage)}", ConsoleColor.Green);
                    Match referencesMatch = Regex.Match(relayXML, @"<references>\s*(.*?)\s*</references>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
                    ColorLine($"[Relayer] {SVDictionary.AlertDetected(Settings.Default.CurrentLanguage)}", ConsoleColor.Green);
                    File.Move("relay.xml", $"{FileHistoryDirectory}\\{resultFileName}");
                    bool IsUI = Settings.Default.WirelessAlertMode;
                    foreach (Match match in Regex.Matches(relayXML, @"<valueName>([^<]+)</valueName>\s*<value>([^<]+)</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline))
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

                    Match sentMatch = Regex.Match(relayXML, @"<sent>\s*(.*?)\s*</sent>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    Match statusMatch = Regex.Match(relayXML, @"<status>\s*(.*?)\s*</status>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    Match messageTypeMatch = Regex.Match(relayXML, @"<msgType>\s*(.*?)\s*</msgType>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    MatchCollection broadcastImmediatelyMatches = Regex.Matches(relayXML, @"<valueName>layer:SOREM:1.0:Broadcast_Immediately</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    MatchCollection urgencyMatches = Regex.Matches(relayXML, @"<urgency>\s*(.*?)\s*</urgency>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    MatchCollection severityMatches = Regex.Matches(relayXML, @"<severity>\s*(.*?)\s*</severity>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    
                    bool final = false;

                    for (int i = 0; i < severityMatches.Count; i++)
                    {
                        if (Check.Config(relayXML, statusMatch.Groups[1].Value, messageTypeMatch.Groups[1].Value, severityMatches[i].Groups[1].Value, urgencyMatches[i].Groups[1].Value, broadcastImmediatelyMatches[i].Groups[1].Value))
                        {
                            final = true;
                            break;
                        }
                    }

                    if (!final)
                    {
                        Console.WriteLine($"[Relayer] {SVDictionary.FileIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}");
                        continue;
                    }

                    MatchCollection infoMatches = Regex.Matches(relayXML, @"<info>\s*(.*?)\s*</info>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    int infoProc = 0;

                    foreach (Match infoMatch in infoMatches)
                    {
                        infoProc++;
                        Console.WriteLine($"[Relayer] Processing match {infoProc} of {infoMatches.Count}.");
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
                            Console.WriteLine($"[Relayer] {SVDictionary.GeneratingProductText(Settings.Default.CurrentLanguage)}");

                            Console.WriteLine($"0: {messageTypeMatch.Groups[0].Value}");
                            Console.WriteLine($"1: {messageTypeMatch.Groups[1].Value}");
                            Console.WriteLine($"2: {messageTypeMatch.Groups[2].Value}");
                            Console.WriteLine($"3: {messageTypeMatch.Groups[3].Value}");
                            Console.WriteLine($"4: {messageTypeMatch.Groups[4].Value}");

                            Generate gen = new Generate(infoEN, sentMatch.Groups[1].Value, DateTime.Now.ToString());

                            var info = gen.BroadcastInfo(lang);

                            if (true) //(!string.IsNullOrWhiteSpace(info.BroadcastText))
                            {
                                //Console.WriteLine($"[Relayer] {SVDictionary.GeneratingProductAudio}");
                                File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", string.Empty);
                                File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", $"{info.BroadcastText}\x20");
                                File.WriteAllText($"{AssemblyDirectory}\\static-text.txt", $"{info.BroadcastText}\x20");

                                Console.WriteLine($"[Relayer] -> {info.BroadcastText}");

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
                                    Console.WriteLine($"[Relayer] {SVDictionary.PlayingAudio(Settings.Default.CurrentLanguage)}");
                                    Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\in.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                    if (EventInfo.Severity.Contains("Severe") || EventInfo.Severity.Contains("Extreme"))
                                        Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                    else
                                    {
                                        var (FilePlayed, AudioLength) = Play($"{AudioDirectory}\\attn-minor.wav");
                                        if (FilePlayed)
                                            Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\attn-minor.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                        else
                                            Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\attn.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                    }
                                    //Console.WriteLine($"[Relayer] Attention tone not played because the alert severity is not severe or extreme.");
                                    Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\audio.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                    Console.WriteLine($"[Relayer] Played {Play($"{AudioDirectory}\\out.wav").AudioLength.TotalMilliseconds} millisecond(s) of audio.");
                                }

                                File.WriteAllText($"{AssemblyDirectory}\\active-text.txt", string.Empty);
                                File.WriteAllText($"{AssemblyDirectory}\\inactive-text.txt", $"{info.BroadcastText}\x20");
                            }
                            else
                            {
                                Console.WriteLine($"[Relayer] {SVDictionary.GeneratedProductEmpty(Settings.Default.CurrentLanguage)}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Relayer] {SVDictionary.FileIgnoredDueToPreferences(Settings.Default.CurrentLanguage)}");
                        }
                    }
                }
            }
        }

        public static void CAP()
        {
            while (true)
            {
                if (new Capture().Main())
                {
                    break;
                }
                Thread.Sleep(5000);
            }
        }

        public static Capture capture;

        private static (bool FilePlayed, TimeSpan AudioLength) Play(string filePath, float volume = 1, bool DefaultDevice = false)
        {
            if (File.Exists(filePath))
            {
                using (var audioFile = new AudioFileReader(filePath))
                using (var outputDevice = new WaveOutEvent
                {
                    Volume = volume
                })
                {
                    if (!DefaultDevice) outputDevice.DeviceNumber = Settings.Default.SoundDevice;
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(100);
                    }
                    return (true, outputDevice.GetPositionTimeSpan());
                }
            }
            else return (false, TimeSpan.Zero);
        }

        public static Thread MethodToThread(Action method)
        {
            return new Thread(() =>
            {
                try
                {
                    method();
                }
                catch (ThreadAbortException ex)
                {
                    Console.WriteLine($"Shutdown caught in thread: {ex.Message}");
                    File.AppendAllText($"{AssemblyDirectory}\\exception.log",
                        $"{DateTime.Now:G} | Shutdown in {method.Method.Name}\r\n" +
                        $"{ex.StackTrace}\r\n" +
                        $"{ex.Source}\r\n" +
                        $"{ex.Message}\r\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception caught in thread: {ex.Message}");
                    File.AppendAllText($"{AssemblyDirectory}\\exception.log",
                        $"{DateTime.Now:G} | Exception in {method.Method.Name}\r\n" +
                        $"{ex.StackTrace}\r\n" +
                        $"{ex.Source}\r\n" +
                        $"{ex.Message}\r\n");
                }
            });
        }

        public static void KeyboardProcessor()
        {
            while (true)
            {
                try
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.D0:
                            Console.WriteLine("What do you want me to do? Divide by zero?");
                            break;
                        case ConsoleKey.Delete:
                            ClearFolder(FileHistoryDirectory);
                            Console.WriteLine("Cleared history folder.");
                            break;
                        case ConsoleKey.G:
                            Console.WriteLine("GUI shown.");
                            break;
                    }
                }
                catch (Exception)
                {

                }
                Thread.Sleep(250);
            }
        }

        public static Thread Watchdog()
        {
            return new Thread(() =>
            {
                string ProgramVersion = Program.FriendlyVersion;
                Init(ProgramVersion, false);
                while (true)
                {
                    Config();
                    Check.LastHeartbeat = DateTime.Now;
                    Program.CAPService = MethodToThread(CAP);
                    Program.CAPService.Start();
                    Program.RelayService = MethodToThread(Relay);
                    Program.RelayService.Start();
                    Program.KeyboardProc = MethodToThread(KeyboardProcessor);
                    Program.KeyboardProc.Start();
                    while (true)
                    {
                        if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 5)
                        {
                            if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 10)
                            {
                                ColorLine($"[Watchdog] {SVDictionary.WatchdogForceRestartingProcess(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);

                                try
                                {
                                    Program.CAPService.Abort();
                                    Program.RelayService.Abort();
                                    capture.ShutdownCapture = true;

                                    while (capture.ShutdownCapture)
                                    {
                                        Thread.Sleep(500);
                                    }

                                    Init(ProgramVersion, true);
                                    Thread.Sleep(5000);
                                }
                                catch (Exception)
                                {
                                    
                                }
                                
                                break;
                            }
                            else ColorLine($"[Watchdog] {SVDictionary.WatchdogForcedRestartWarning(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);
                        }
                        Thread.Sleep(5000);
                    }
                }
            });
        }
    }

    internal static class Program
    {
        public const int ReleaseVersion = 1;
        public const int MinorVersion = 1;
        public const bool IsCuttingEdge = true;
        public static string FriendlyVersion = string.Empty;

        private static readonly Thread WatchdogService = ENDEC.Watchdog();
        internal static Thread CAPService;
        internal static Thread RelayService;
        internal static Thread KeyboardProc;
        internal static Thread BatteryProc;

        [STAThread]
        static void Main()
        {
            Console.CancelKeyPress += CancelAllOperations;
            SetVersion();
            //if (IsCuttingEdge) Console.WriteLine("You are running on SharpENDEC Cutting Edge, which may be unstable.");
            // PLUGIN IMPLEMENTATION!!!
            // BATTERY IMPLEMENTATION!!!
            // GUI IMPLEMENTATION!!!
            WatchdogService.Start();
        }

        private static void SetVersion()
        {
            if (!IsCuttingEdge)
            {
                FriendlyVersion = $"SharpENDEC {ReleaseVersion}.{MinorVersion} | Release";
            }
            else
            {
                FriendlyVersion = $"SharpENDEC {ReleaseVersion}.{MinorVersion} | Cutting Edge (unstable)";
            }
        }

        private static void CancelAllOperations(object sender, ConsoleCancelEventArgs e)
        {
            WatchdogService.Abort();
            CAPService.Abort();
            RelayService.Abort();
            ENDEC.capture.ShutdownCapture = true;
            Console.WriteLine("(-^-)");
            Thread.Sleep(2500);
            Environment.Exit(0);
        }
    }

    public static class SVDictionary
    {
        public static string RecoveredFromFailure(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le chien de garde a récemment détecté un problème et a effacé toutes les données XML stockées.\r\n" +
                           "Les alertes précédemment relayées peuvent être relayées à nouveau lorsque le prochain battement de coeur arrive.";
                case "en":
                default:
                    return "The watchdog has recently detected a problem, and has cleared all XML data stored.\r\n" +
                           "Alerts previously relayed may relay again when the next heartbeat arrives.";
            }
        }

        public static string WatchdogForcedRestartWarning(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Cela fait plus de 5 minutes depuis le dernier battement de coeur. Si aucun battement de coeur n'est détecté dans les 5 minutes supplémentaires, le programme redémarrera automatiquement.";
                case "en":
                default:
                    return "It has been more than 5 minutes since the last heartbeat. If a heartbeat is not detected within 5 additional minutes, the program will automatically restart.";
            }
        }

        public static string WatchdogForceRestartingProcess(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le battement de coeur a été présumé mort. Redémarrage de tous les services dans quelques instants.";
                case "en":
                default:
                    return "The heartbeat has been presumed dead. Restarting all services in a few moments.";
            }
        }

        public static string WatchingFiles(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Surveillance du répertoire pour les alertes.";
                case "en":
                default:
                    return "Watching directory for alerts.";
            }
        }

        public static string HeartbeatDetected(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Battement de coeur détecté.";
                case "en":
                default:
                    return "Heartbeat detected.";
            }
        }

        public static string AlertDetected(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Alerte détectée.";
                case "en":
                default:
                    return "Alert detected.";
            }
        }

        public static string ConnectedToServer(string lang, string host, int port)
        {
            switch (lang)
            {
                case "fr":
                    return $"Connecté à {host} sur le port {port}.";
                case "en":
                default:
                    return $"Connected to {host} on port {port}.";
            }
        }

        public static string HostTimedOut(string lang, string host)
        {
            switch (lang)
            {
                case "fr":
                    return $"{host} n'a envoyé aucune donnée dans le délai minimum.";
                case "en":
                default:
                    return $"{host} hasn't sent any data within the minimum time limit.";
            }
        }

        public static string RestartingAfterException(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Le fil de capture est mort de façon inattendue. Il redémarrera automatiquement dans quelques instants.";
                case "en":
                default:
                    return "The capture thread has died unexpectedly. It will automatically restart in a few moments.";
            }
        }

        public static string FileDownloaded(string lang, string directory, string filename, string host)
        {
            switch (lang)
            {
                case "fr":
                    return $"Fichier enregistré : {directory}\\{filename} | De : {host}";
                case "en":
                default:
                    return $"File saved: {directory}\\{filename} | From: {host}";
            }
        }

        public static string DownloadingFiles(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Téléchargement de fichiers à partir du signal cardiaque reçu.";
                case "en":
                default:
                    return "Downloading files from received heartbeat.";
            }
        }

        public static string FileIgnoredDueToMatchingPair(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Paire correspondante détectée. Ce fichier ne sera pas traité davantage.";
                case "en":
                default:
                    return "Matching pair detected. This file won't be processed.";
            }
        }

        public static string FilesIgnoredDueToMatchingPairs(string lang, int count)
        {
            switch (lang)
            {
                case "fr":
                    return $"{count} fichier(s) avaient des paires correspondantes et n'ont pas été traités.";
                case "en":
                default:
                    return $"{count} file(s) had matching pairs, and were not processed.";
            }
        }

        public static string FileIgnoredDueToPreferences(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Les préférences ne permettent pas le relais de cette alerte. Ce fichier ne sera pas traité davantage.";
                case "en":
                default:
                    return "Preferences do not allow the relay of this alert. This file won't be processed.";
            }
        }

        public static string GeneratingProductText(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Génération de texte en cours.";
                case "en":
                default:
                    return "Generating text.";
            }
        }

        public static string GeneratingProductAudio(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Génération audio en cours.";
                case "en":
                default:
                    return "Generating audio.";
            }
        }

        public static string GeneratedProductEmpty(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Il n’y avait rien à générer.";
                case "en":
                default:
                    return "There was nothing to generate.";
            }
        }

        public static string PlayingAudio(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "La lecture audio a commencé.";
                case "en":
                default:
                    return "Audio playback started.";
            }
        }

        public static string CapturedFromFileWatcher(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "File Watcher a capturé un fichier :";
                case "en":
                default:
                    return "File Watcher captured a file:";
            }
        }

        public static string ProcessingStream(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Flux de données de traitement.";
                case "en":
                default:
                    return "Processing data stream.";
            }
        }

        public static string ProcessedStream(string lang, int total, DateTime started)
        {
            switch (lang)
            {
                case "fr":
                    return $"Traité {total} dans {(DateTime.Now - started).TotalSeconds}s.";
                case "en":
                default:
                    return $"Processed {total} in {(DateTime.Now - started).TotalSeconds}s.";
            }
        }

        public static string LastDataReceived(string lang)
        {
            switch (lang)
            {
                case "fr":
                    return "Dernières données reçues :";
                case "en":
                default:
                    return "Last data received:";
            }
        }
    }

    public static class SVDictionaryLegacy
    {
        private static bool isFrench = false;

        public static bool IsFrench
        {
            get => isFrench;
            set => isFrench = value;
        }

        public static string RecoveredFromFailure
        {
            get => IsFrench
                ? "Le chien de garde a récemment détecté un problème et a effacé toutes les données XML stockées.\r\n" +
                "Les alertes précédemment relayées peuvent être relayées à nouveau lorsque le prochain battement de coeur arrive."
                : "The watchdog has recently detected a problem, and has cleared all XML data stored.\r\n" +
                "Alerts previously relayed may relay again when the next heartbeat arrives.";
        }
        
        public static string WatchdogForcedRestartWarning
        {
            get => IsFrench
                ? "Cela fait plus de 5 minutes depuis le dernier battement de coeur. Si aucun battement de coeur n'est détecté dans les 5 minutes supplémentaires, le programme redémarrera automatiquement."
                : "It has been more than 5 minutes since the last heartbeat. If a heartbeat is not detected within 5 additional minutes, the program will automatically restart.";
        }
        
        public static string WatchdogForceRestartingProcess
        {
            get => IsFrench
                ? "Le battement de coeur a été présumé mort. Redémarrage de tous les services dans quelques instants."
                : "The heartbeat has been presumed dead. Restarting all services in a few moments.";
        }
        
        public static string WatchingFiles
        {
            get => IsFrench
                ? "Surveillance du répertoire pour les alertes."
                : "Watching directory for alerts.";
        }
        
        public static string HeartbeatDetected
        {
            get => IsFrench
                ? "Battement de coeur détecté."
                : "Heartbeat detected.";
        }
        
        public static string AlertDetected
        {
            get => IsFrench
                ? "Battement de coeur détecté."
                : "Alert detected.";
        }
        
        public static string ConnectedToServer(string host, int port)
        {
            return IsFrench
                ? $"Connecté à {host} sur le port {port}."
                : $"Connected to {host} on port {port}.";
        }
        
        public static string HostTimedOut(string host)
        {
            return IsFrench
                ? $"{host} n'a envoyé aucune donnée dans le délai minimum."
                : $"{host} hasn't sent any data within the minimum time limit.";
        }
        
        public static string RestartingAfterException
        {
            get => IsFrench
                ? "Le fil de capture est mort de façon inattendue. Il redémarrera automatiquement dans quelques instants."
                : "The capture thread has died unexpectedly. It will automatically restart in a few moments.";
        }

        public static string FileDownloaded(string directory, string filename, string host)
        {
            return IsFrench
                ? $"Fichier enregistré : {directory}\\{filename} | De : {host}"
                : $"File saved: {directory}\\{filename} | From: {host}";
        }

        public static string DownloadingFiles
        {
            get => IsFrench
                ? "Téléchargement de fichiers à partir du signal cardiaque reçu."
                : "Downloading files from received heartbeat.";
        }
        
        public static string FileIgnoredDueToMatchingPair
        {
            get => IsFrench
                ? "Paire correspondante détectée. Ce fichier ne sera pas traité davantage."
                : "Matching pair detected. This file won't be processed.";
        }

        public static string FilesIgnoredDueToMatchingPairs(int count)
        {
            return IsFrench
                ? $"{count} fichier(s) avaient des paires correspondantes et n'ont pas été traités."
                : $"{count} file(s) had matching pairs, and were not processed.";
        }

        public static string FileIgnoredDueToPreferences
        {
            get => IsFrench
                ? "Les préférences ne permettent pas le relais de cette alerte. Ce fichier ne sera pas traité davantage."
                : "Preferences do not allow the relay of this alert. This file won't be processed.";
        }
        
        public static string GeneratingProductText
        {
            get => IsFrench
                ? "Génération de texte en cours."
                : "Generating text.";
        }
        
        public static string GeneratingProductAudio
        {
            get => IsFrench
                ? "Génération audio en cours."
                : "Generating audio.";
        }
        
        public static string GeneratedProductEmpty
        {
            get => IsFrench
                ? "Il n’y avait rien à générer."
                : "There was nothing to generate.";
        }
        
        public static string PlayingAudio
        {
            get => IsFrench
                ? "La lecture audio a commencé."
                : "Audio playback started.";
        }

        public static string CapturedFromFileWatcher
        {
            get => IsFrench
                ? "File Watcher a capturé un fichier :"
                : "File Watcher captured a file:";
        }
        
        public static string ProcessingStream
        {
            get => IsFrench
                ? "Flux de données de traitement."
                : "Processing data stream.";
        }

        public static string ProcessedStream(int total, DateTime started)
        {
            return IsFrench
                ? $"Traité {total} dans {(DateTime.Now - started).TotalSeconds}s."
                : $"Processed {total} in {(DateTime.Now - started).TotalSeconds}s.";
        }

        public static string LastDataReceived
        {
            get => IsFrench
                ? "Dernières données reçues :"
                : "Last data received:";
        }
    }
}