using NAudio.Wave;
using SharpENDEC.Properties;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Utils;

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

                    if (relayItem.IsNull()) continue;

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
                    ConsoleExt.WriteLine($"[Audio Player] -> {filePath}.");
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
                    catch (Exception)
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
                            ConsoleExt.WriteLineErr($"[Data Processor] {ex.Message}");
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
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"Mozilla/5.0 (compatible; SharpENDEC/{VersionInfo.ReleaseVersion}.{VersionInfo.MinorVersion})");
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
                            // figure out something here to avoid deadlocking
                            xml.Wait();
                            lock (SharpDataQueue) SharpDataQueue.Add(new SharpDataItem(filename, xml.Result));
                            xml.Dispose();
                        }
                        catch (Exception e1)
                        {
                            try
                            {
                                ConsoleExt.WriteLineErr($"[Heartbeat] {e1.Message}");
                                ConsoleExt.WriteLine($"[Heartbeat] {filename}...", ConsoleColor.Yellow);
                                ConsoleExt.WriteLine($"-> {url2}", ConsoleColor.Yellow);
                                Task<string> xml = client.GetStringAsync(url2);
                                // figure out something here to avoid deadlocking
                                xml.Wait();
                                lock (SharpDataQueue) SharpDataQueue.Add(new SharpDataItem(filename, xml.Result));
                                xml.Dispose();
                            }
                            catch (Exception e2)
                            {
                                ConsoleExt.WriteLineErr($"[Heartbeat] {e2.Message}");
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
                    Thread.Sleep(100);
                }
            }
        }
    }
}
