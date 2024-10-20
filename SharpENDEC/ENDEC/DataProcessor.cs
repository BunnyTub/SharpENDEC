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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Utils;
using System.Collections.Generic;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static void DataProcessor()
        {
            //try
            {
                while (true)
                {
                    SharpDataItem relayItem = Check.WatchForItemsInList();
                    ConsoleExt.WriteLine($"[Data Processor] {LanguageStrings.CapturedFromFileWatcher(Settings.Default.CurrentLanguage)}", ConsoleColor.Cyan);

                    lock (SharpDataHistory) SharpDataHistory.Add(relayItem);
                    lock (SharpDataQueue) SharpDataQueue.Remove(relayItem);

                    if (relayItem.Data.Contains("<sender>NAADS-Heartbeat</sender>"))
                    {
                        ConsoleExt.WriteLine($"[Data Processor] {LanguageStrings.HeartbeatDetected(Settings.Default.CurrentLanguage)}", ConsoleColor.Green);
                        Match referencesMatch = Regex.Match(relayItem.Data, @"<references>\s*(.*?)\s*</references>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        if (referencesMatch.Success)
                        {
                            string references = referencesMatch.Groups[1].Value;
                            //ConsoleExt.WriteLine(referencesMatch.Groups[0].Value);
                            //ConsoleExt.WriteLine(referencesMatch.Groups[1].Value);
                            Check.Heartbeat(references);
                        }
                    }
                    else
                    {
                        lock (SharpAlertQueue) SharpAlertQueue.Add(relayItem);
                        ConsoleExt.WriteLine(LanguageStrings.AlertQueued(Settings.Default.CurrentLanguage));
                    }

                    ConsoleExt.WriteLine($"[Data Processor] {LanguageStrings.LastDataReceived(Settings.Default.CurrentLanguage)} {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
                }
            }
            //catch (ThreadAbortException)
            //{
            //    ConsoleExt.WriteLine($"{LanguageStrings.ThreadShutdown(Settings.Default.CurrentLanguage, $"Data Processor")}");
            //}
        }

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
                    ConsoleExt.WriteLine($"[Data Processor] -> {filePath}.");
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        if (SkipPlayback)
                        {
                            outputDevice.Stop();
                            SkipPlayback = false;
                        }
                        if (outputDevice.GetPositionTimeSpan() >= TimeSpan.FromSeconds(Settings.Default.EnforceMaximumTime)
                            && Settings.Default.EnforceMaximumTime != -1)
                            outputDevice.Stop();
                        Thread.Sleep(100);
                    }
                    return (true, outputDevice.GetPositionTimeSpan());
                }
            }
            else return (false, TimeSpan.Zero);
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
                            ConsoleExt.WriteLine($"[Data Processor] {ex.Message}", ConsoleColor.Red);
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
                ConsoleExt.WriteLine($"[Heartbeat] {LanguageStrings.DownloadingFiles(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkYellow);
                string[] RefList = References.Split(' ');
                int DataMatched = 0;
                int Total = 0;
                foreach (string i in RefList)
                {
                    Total++;
                    ConsoleExt.WriteLine($"[Heartbeat] {LanguageStrings.GenericProcessingValueOfValue(Settings.Default.CurrentLanguage, Total, RefList.Length)}", ConsoleColor.DarkYellow);
                    string filename = string.Empty;
                    string[] k = i.Split(',');
                    string sentDTFull = k[2].Replace("-", "_").Replace(":", "_").Replace("+", "p");
                    string sentDT = sentDTFull.Substring(0, 10).Replace("_", "-");
                    filename += sentDTFull;
                    filename += "I" + k[1].Replace("-", "_").Replace(":", "_");
                    filename += ".xml";
                    //string filenameShort = sentDTFull.Replace("_", "-");

                    string Dom1 = "capcp1.naad-adna.pelmorex.com";
                    string Dom2 = "capcp2.naad-adna.pelmorex.com";

                    if (SharpDataQueue.Any(x => x.Name == filename) || SharpDataHistory.Any(x => x.Name == filename))
                    {
                        ConsoleExt.WriteLine($"[Heartbeat] {LanguageStrings.DataPreviouslyProcessed(Settings.Default.CurrentLanguage)}", ConsoleColor.Yellow);
                        DataMatched++;
                    }
                    else
                    {
                        ConsoleExt.WriteLine($"[Heartbeat] {filename}...", ConsoleColor.Yellow);
                        string url1 = $"http://{Dom1}/{sentDT}/{filename}";
                        string url2 = $"http://{Dom2}/{sentDT}/{filename}";

                        try
                        {
                            ConsoleExt.WriteLine($"-> {url1}", ConsoleColor.Yellow);
                            Task<string> xml = client.GetStringAsync(url1);
                            xml.Wait();
                            lock (SharpDataQueue) SharpDataQueue.Add(new SharpDataItem(filename, xml.Result));
                            xml.Dispose();
                        }
                        catch (Exception e1)
                        {
                            try
                            {
                                ConsoleExt.WriteLine($"[Heartbeat] {e1.Message}", ConsoleColor.Red);
                                ConsoleExt.WriteLine($"[Heartbeat] {filename}...", ConsoleColor.Yellow);
                                ConsoleExt.WriteLine($"-> {url2}", ConsoleColor.Yellow);
                                Task<string> xml = client.GetStringAsync(url2);
                                xml.Wait();
                                lock (SharpDataQueue) SharpDataQueue.Add(new SharpDataItem(filename, xml.Result));
                                xml.Dispose();
                            }
                            catch (Exception e2)
                            {
                                ConsoleExt.WriteLine($"[Heartbeat] {e2.Message}", ConsoleColor.Red);
                                ConsoleExt.WriteLine($"[Heartbeat] {LanguageStrings.DownloadFailure(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);
                            }
                        }
                    }
                }
                if (DataMatched != 0) ConsoleExt.WriteLine($"[Heartbeat] {LanguageStrings.DataIgnoredDueToMatchingPairs(Settings.Default.CurrentLanguage, DataMatched)}", ConsoleColor.Blue);
                LastHeartbeat = DateTime.Now;
            }

            public static SharpDataItem WatchForItemsInList()
            {
                //ConsoleExt.WriteLine($"[Data Processor] Watching for new strings in FileStringListTempName.");
                while (true)
                {
                    if (SharpDataQueue.Count != 0)
                    {
                        lock (SharpDataQueue)
                        {
                            SharpDataItem data = SharpDataQueue.First();
                            return data;
                        }
                    }
                    Thread.Sleep(50);
                }
                return null;
            }
        }

        public class Generate
        {
            private readonly string InfoData;
            private readonly string MsgType;
            private readonly string Sent;

            public Generate(string InfoDataZ, string MsgTypeZ, string SentDate)
            {
                InfoData = InfoDataZ;
                MsgType = MsgTypeZ;
                Sent = SentDate;
            }

            public (string BroadcastText, bool) BroadcastInfo(string lang)
            {
                string BroadcastText = "";

                Match match = Regex.Match(InfoData, @"<valueName>layer:SOREM:1.0:Broadcast_Text</valueName>\s*<value>\s*(.*?)\s*</value>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

                    DateTime sentDate;
                    try
                    {
                        // .ToUniversalTime()
                        sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    }
                    catch (Exception e)
                    {
                        ConsoleExt.WriteLine(e.Message);
                        sentDate = DateTime.Now;
                    }

                    //DateTime sentDate = DateTime.Parse(Sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

                    string SentFormatted = lang == "fr" ? $"{sentDate:HH}'{sentDate:h}'{sentDate:mm zzz}." : $"{sentDate:HH:mm z}, {sentDate:MMMM dd}, {sentDate:yyyy}.";

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
                        Description = Regex.Match(InfoData, @"<description>\s*(.*?)\s*</description>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        if (!Description.EndsWith(".")) Description += ".";
                    }
                    catch (Exception)
                    {
                        Description = "";
                    }

                    string Instruction;

                    try
                    {
                        Instruction = Regex.Match(InfoData, @"<instruction>\s*(.*?)\s*</instruction>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        if (!Instruction.EndsWith(".")) Instruction += ".";
                    }
                    catch (Exception)
                    {
                        Instruction = "";
                    }

                    string Effective;

                    try
                    {
                        Effective = Regex.Match(InfoData, @"<effective>\s*(.*?)\s*</effective>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        DateTime.Parse(Effective, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        Effective = "currently";
                    }
                    
                    string Expires;

                    try
                    {
                        Expires = Regex.Match(InfoData, @"<expires>\s*(.*?)\s*</expires>", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value.Replace("\n", " ");
                        DateTime.Parse(Expires, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        Expires = "soon";
                    }

                    // Effective {Effective}, and expiring {Expires}.

                    BroadcastText = lang == "fr" ?
                        $"À {SentFormatted} {SenderName} a {MsgPrefix} une {EventType} {Coverage} {AreaDesc}. {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim() :
                        $"At {SentFormatted} {SenderName} has {MsgPrefix} a {EventType} {Coverage} {AreaDesc}. {Description} {Instruction}".Replace("###", "").Replace("  ", " ").Trim();
                }

                if (BroadcastText.EndsWith("\x20.")) BroadcastText = BroadcastText.TrimEnd('\x20', '.');
                if (BroadcastText.EndsWith(".")) BroadcastText = BroadcastText.TrimEnd('.');
                if (!BroadcastText.EndsWith(".") || !BroadcastText.EndsWith("!")) BroadcastText += ".";

                //if (Debugger.IsAttached) BroadcastText += "\x20| Debugging in progress";

                return (BroadcastText, true);
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
