using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static SharpENDEC.VersionInfo;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string AudioDirectory = $"{AssemblyDirectory}\\Audio";
        public static readonly string FileQueueDirectory = $"{AssemblyDirectory}\\FileQueue";
        public static readonly string FileHistoryDirectory = $"{AssemblyDirectory}\\FileHistory";
        private static readonly Random rnd = new Random();

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

        public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
                                       && WindowsIdentity.GetCurrent().Owner == WindowsIdentity.GetCurrent().User;
        public static bool IsGuest => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Guest);

        /// <summary>
        /// This method should only be called when the program is started or restarted.
        /// </summary>
        public static void Init(string VersionID, bool RecoveredFromProblem)
        {
            Console.Title = VersionID;
            Console.WriteLine($"{VersionID}\r\n" +
                $"Ported from ApatheticDELL's QuantumENDEC 4.\r\n\r\n" +
                $"Project created by BunnyTub.\r\n" +
                $"Logo created by ApatheticDELL.\r\n" +
                $"Translations may not be 100% accurate due to language deviations.\r\n");

            if (RecoveredFromProblem) ColorLine(LanguageStrings.RecoveredFromFailure(Settings.Default.CurrentLanguage), ConsoleColor.DarkRed);
            if (IsAdministrator) ColorLine(LanguageStrings.ElevationSecurityProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);
            if (IsGuest) ColorLine(LanguageStrings.ConfigurationLossProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);

            CheckFolder(FileQueueDirectory, RecoveredFromProblem);
            CheckFolder(FileHistoryDirectory, RecoveredFromProblem);
            CheckFolder("Audio", false);

            if (!File.Exists($"{AudioDirectory}\\attn.wav"))
            {
                MemoryStream mem = new MemoryStream();
                Resources.attn.CopyTo(mem);
                File.WriteAllBytes("Audio\\attn.wav", mem.ToArray());
                mem.Dispose();
                Console.WriteLine("The attention tone audio \"attn.wav\" doesn't exist. The default one will be used instead.");
            }

            //Console.WriteLine();
            Console.WriteLine($"Press SPACE to pause for 30 seconds.");

            bool alreadyPaused = false;

            for (int i = 5; i > 0; i--)
            {
                if (alreadyPaused) break;
                Thread.Sleep(1000);

                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Spacebar:
                            Console.WriteLine("Paused for 30 seconds.");
                            Thread.Sleep(30000);
                            alreadyPaused = true;
                            continue;
                    }
                }
            }
            Console.Clear();
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

        public static void StreamProcessor()
        {
            try
            {
                while (true)
                {
                    if (new FeedCapture().Main())
                    {
                        break;
                    }
                    Thread.Sleep(5000);
                }
                Console.WriteLine("Stream Processor was stopped.");
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Stream Processor was stopped.");
            }
        }

        public static FeedCapture capture;

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

        //public static Thread MethodToThread(Action method)
        //{
        //    return new Thread(() =>
        //    {
        //        try
        //        {
        //            method();
        //        }
        //        catch (ThreadAbortException ex)
        //        {
        //            Console.WriteLine($"Shutdown caught in thread: {ex.Message}");
        //            File.AppendAllText($"{AssemblyDirectory}\\exception.log",
        //                $"{DateTime.Now:G} | Shutdown in {method.Method.Name}\r\n" +
        //                $"{ex.StackTrace}\r\n" +
        //                $"{ex.Source}\r\n" +
        //                $"{ex.Message}\r\n");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Exception caught in thread: {ex.Message}");
        //            File.AppendAllText($"{AssemblyDirectory}\\exception.log",
        //                $"{DateTime.Now:G} | Exception in {method.Method.Name}\r\n" +
        //                $"{ex.StackTrace}\r\n" +
        //                $"{ex.Source}\r\n" +
        //                $"{ex.Message}\r\n");
        //        }
        //    });
        //}

        public static void KeyboardProcessor()
        {
            try
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
            catch (ThreadAbortException)
            {
                Console.WriteLine("Keyboard Processor was stopped.");
            }
        }

        public static Thread Watchdog()
        {
            return new Thread(() =>
            {
                string ProgramVersion = FriendlyVersion;
                Init(ProgramVersion, false);

                void RestartThread(ref Thread serviceThread, ThreadStart method)
                {
                    if (serviceThread != null)
                    {
                        if (serviceThread.ThreadState != ThreadState.Stopped && serviceThread.ThreadState != ThreadState.Unstarted)
                            serviceThread.Abort();
                    }

                    serviceThread = new Thread(method);
                    serviceThread.Start();
                }

                void ShutdownCapture()
                {
                    capture.ShutdownCapture = true;
                    Console.WriteLine("Waiting for File Processor to shutdown.");
                    while (capture.ShutdownCapture)
                    {
                        Thread.Sleep(500);
                    }
                    Console.WriteLine("File Processor has been stopped.");
                }

                while (true)
                {
                    Config();
                    Check.LastHeartbeat = DateTime.Now;

                    RestartThread(ref Program.StreamService, StreamProcessor);
                    RestartThread(ref Program.RelayService, FileProcessor);
                    RestartThread(ref Program.KeyboardProc, KeyboardProcessor);

                    while (true)
                    {
                        if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 5)
                        {
                            if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 10)
                            {
                                ColorLine($"[Watchdog] {LanguageStrings.WatchdogForceRestartingProcess(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);

                                try
                                {
                                    ShutdownCapture();
                                    RestartThread(ref Program.StreamService, StreamProcessor);
                                    RestartThread(ref Program.RelayService, FileProcessor);
                                    RestartThread(ref Program.KeyboardProc, KeyboardProcessor);

                                    Init(ProgramVersion, true);
                                    Thread.Sleep(5000);
                                }
                                catch (Exception)
                                {
                                    
                                }
                                
                                break;
                            }
                            else ColorLine($"[Watchdog] {LanguageStrings.WatchdogForcedRestartWarning(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);
                        }
                        Thread.Sleep(5000);
                    }
                }
            });
        }
    }

    internal static class Program
    {
        internal static Thread WatchdogService;
        internal static Thread StreamService;
        internal static Thread RelayService;
        internal static Thread KeyboardProc;
        internal static Thread BatteryProc;

        [STAThread]
        private static void Main()
        {
            Console.ForegroundColor = ConsoleColor.White;
            // PLUGIN IMPLEMENTATION!!!
            // BATTERY IMPLEMENTATION!!!
            // GUI IMPLEMENTATION!!!
            WatchdogService = ENDEC.Watchdog();
            WatchdogService.Start();
            Console.CancelKeyPress += CancelAllOperations;
        }

        private static void CancelAllOperations(object sender, ConsoleCancelEventArgs e)
        {
            WatchdogService.Abort();
            StreamService.Abort();
            RelayService.Abort();
            ENDEC.capture.ShutdownCapture = true;
            Console.WriteLine("(-^-)");
            Thread.Sleep(2500);
            Environment.Exit(0);
        }
    }
}