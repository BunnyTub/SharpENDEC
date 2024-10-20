using NAudio.Wave;
using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Speech.Synthesis;
using System.Threading;
using static SharpENDEC.VersionInfo;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string AudioDirectory = $"{AssemblyDirectory}\\Audio";

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

        public static bool SkipPlayback = false;

        /// <summary>
        /// This method should only be called when the program is started or restarted.
        /// </summary>
        public static void Init(string VersionID, bool RecoveredFromProblem)
        {
            Console.Title = VersionID;
            ConsoleExt.WriteLine($"{VersionID}\r\n" +
                $"Source code forked from QuantumENDEC 4.\r\n\r\n" +
                $"Project created by BunnyTub.\r\n" +
                $"Logo created by ApatheticDELL.\r\n" +
                $"Translations may not be 100% accurate due to language deviations.\r\n");

            if (RecoveredFromProblem) ConsoleExt.WriteLine(LanguageStrings.RecoveredFromFailure(Settings.Default.CurrentLanguage), ConsoleColor.DarkRed);
            if (IsAdministrator) ConsoleExt.WriteLine(LanguageStrings.ElevationSecurityProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);
            if (IsGuest) ConsoleExt.WriteLine(LanguageStrings.ConfigurationLossProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);

            CheckFolder("Audio", false);

            if (!File.Exists($"{AudioDirectory}\\attn.wav"))
            {
                MemoryStream mem = new MemoryStream();
                Resources.attn.CopyTo(mem);
                File.WriteAllBytes("Audio\\attn.wav", mem.ToArray());
                mem.Dispose();
                ConsoleExt.WriteLine("The attention tone audio \"attn.wav\" doesn't exist. The default one will be used instead.");
            }

            //ConsoleExt.WriteLine();
            ConsoleExt.WriteLine($"Press SPACE to pause for 30 seconds.");

            bool alreadyPaused = false;

            for (int i = 3; i > 0; i--)
            {
                if (alreadyPaused) break;
                Thread.Sleep(1000);

                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Spacebar:
                            ConsoleExt.WriteLine("Paused for 30 seconds.");
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
            Settings.Default.Upgrade();
            for (int i = 3; i > 0; i--)
            {
                Console.Clear();
                ConsoleExt.WriteLine($"Press C to configure settings. Press R to reset settings. Continuing otherwise in {i} second(s).");

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
                                ConsoleExt.WriteLine($"--- Sound ---");
                                ConsoleExt.WriteLine($"1. SoundDevice = {Settings.Default.SoundDevice} | Use the specified sound device for audio output");
                                ConsoleExt.WriteLine($"2. SpeechVoice = {Settings.Default.SpeechVoice} | Use the specified voice for TTS when audio is not provided");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Console.Clear();
                                        for (int n = -1; n < WaveOut.DeviceCount; n++)
                                        {
                                            var caps = WaveOut.GetCapabilities(n);
                                            ConsoleExt.WriteLine($"{n}: {caps.ProductName}");
                                        }
                                        Console.Write("Enter sound device ID:\x20");
                                        Settings.Default.SoundDevice = int.Parse(Console.ReadLine());
                                        break;
                                    case ConsoleKey.D2:
                                        Console.Clear();
                                        foreach (var voice in new SpeechSynthesizer().GetInstalledVoices()) ConsoleExt.WriteLine(voice.VoiceInfo.Name);
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
                                ConsoleExt.WriteLine($"--- Statuses ---");
                                ConsoleExt.WriteLine($"1. statusTest = {Settings.Default.statusTest} | Relay test alerts");
                                ConsoleExt.WriteLine($"2. statusActual = {Settings.Default.statusActual} | Relay actual alerts");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
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
                                ConsoleExt.WriteLine($"--- Types ---");
                                ConsoleExt.WriteLine($"1. messageTypeAlert = {Settings.Default.messageTypeAlert} | Relay alert messages");
                                ConsoleExt.WriteLine($"2. messageTypeUpdate = {Settings.Default.messageTypeUpdate} | Relay update messages");
                                ConsoleExt.WriteLine($"3. messageTypeCancel = {Settings.Default.messageTypeCancel} | Relay cancel messages");
                                ConsoleExt.WriteLine($"4. messageTypeTest = {Settings.Default.messageTypeTest} | Relay test messages");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
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
                                ConsoleExt.WriteLine($"--- Severities ---");
                                ConsoleExt.WriteLine($"1. severityExtreme = {Settings.Default.severityExtreme} | Relay extreme messages");
                                ConsoleExt.WriteLine($"2. severitySevere = {Settings.Default.severitySevere} | Relay severe messages");
                                ConsoleExt.WriteLine($"3. severityModerate = {Settings.Default.severityModerate} | Relay moderate messages");
                                ConsoleExt.WriteLine($"4. severityMinor = {Settings.Default.severityMinor} | Relay minor messages");
                                ConsoleExt.WriteLine($"5. severityUnknown = {Settings.Default.severityUnknown} | Relay unknown messages");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
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
                                ConsoleExt.WriteLine($"--- Urgency ---");
                                ConsoleExt.WriteLine($"1. urgencyImmediate = {Settings.Default.urgencyImmediate} | Relay immediate urgency messages");
                                ConsoleExt.WriteLine($"2. urgencyExpected = {Settings.Default.urgencyExpected} | Relay expected urgency messages");
                                ConsoleExt.WriteLine($"3. urgencyFuture = {Settings.Default.urgencyFuture} | Relay future urgency messages");
                                ConsoleExt.WriteLine($"4. urgencyPast = {Settings.Default.urgencyPast} | Relay past urgency messages");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
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
                                ConsoleExt.WriteLine($"--- Geocodes ---");
                                ConsoleExt.WriteLine();
                                foreach (string geo in Settings.Default.AllowedLocations_Geocodes) ConsoleExt.WriteLine(geo);
                                if (Settings.Default.AllowedLocations_Geocodes.Count == 0) ConsoleExt.WriteLine("All");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"1. AllowedLocations_Geocodes | Relay messages with the specified geocodes");
                                ConsoleExt.WriteLine($"2. Relay messages with any location values");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
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
                                ConsoleExt.WriteLine($"--- Relay ---");
                                ConsoleExt.WriteLine($"EnforceMaximumTime = {Settings.Default.EnforceMaximumTime} | Enforce a maximum playback time limit (in seconds)");
                                ConsoleExt.WriteLine($"1. 60 seconds");
                                ConsoleExt.WriteLine($"2. 120 seconds");
                                ConsoleExt.WriteLine($"3. 240 seconds");
                                ConsoleExt.WriteLine($"4. Do not enforce any maximum");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to finish.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.EnforceMaximumTime = 60;
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.EnforceMaximumTime = 120;
                                        break;
                                    case ConsoleKey.D3:
                                        Settings.Default.EnforceMaximumTime = 240;
                                        break;
                                    case ConsoleKey.D4:
                                        Settings.Default.EnforceMaximumTime = -1;
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
                                ConsoleExt.WriteLine($"--- Event Blacklist ---");
                                ConsoleExt.WriteLine();
                                foreach (string item in Settings.Default.EnforceEventBlacklist) ConsoleExt.WriteLine(item);
                                if (Settings.Default.EnforceEventBlacklist.Count == 0) ConsoleExt.WriteLine("None");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"1. Add to blacklist | Don't relay messages with the specified event codes");
                                ConsoleExt.WriteLine($"2. Clear the blacklist");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to go to the next page.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Console.Clear();
                                        Console.Write("Enter code to add:\x20");
                                        Settings.Default.EnforceEventBlacklist.Add(Console.ReadLine());
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.EnforceEventBlacklist.Clear();
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
                                ConsoleExt.WriteLine($"--- Language ---");
                                ConsoleExt.WriteLine($"CurrentLanguage = {Settings.Default.CurrentLanguage} | Use the program in a different language");
                                ConsoleExt.WriteLine($"1. English | Use the program in English");
                                ConsoleExt.WriteLine($"2. Français | Use the program in English");
                                ConsoleExt.WriteLine($"3. ᐃᓄᒃᑎᑐᑦ | ᐃᓄᒃᑎᑐᑦ ᐱᓕᕆᐊᒃᓴᒥᒃ ᐊᑐᖅᐸᒡᓗᒋᑦ");
                                ConsoleExt.WriteLine();
                                ConsoleExt.WriteLine($"Press ENTER to finish.");
                                while (!Console.KeyAvailable) Thread.Sleep(100);
                                switch (Console.ReadKey(true).Key)
                                {
                                    case ConsoleKey.D1:
                                        Settings.Default.CurrentLanguage = "english";
                                        break;
                                    case ConsoleKey.D2:
                                        Settings.Default.CurrentLanguage = "french";
                                        break;
                                    case ConsoleKey.D3:
                                        Settings.Default.CurrentLanguage = "inuktitut";
                                        break;
                                    case ConsoleKey.Enter:
                                        ProgressToNextPage = true;
                                        break;
                                }
                            }

                            Console.Clear();
                            Settings.Default.Save();
                            ConsoleExt.WriteLine("Your preferences have been saved.");
                            Thread.Sleep(2500);
                            Console.Clear();
                            return;
                        case ConsoleKey.R:
                            Console.Clear();
                            Settings.Default.Reset();
                            ConsoleExt.WriteLine("All preferences have been restored to their default configuration.");
                            Thread.Sleep(2500);
                            Console.Clear();
                            return;
                    }
                }
            }
            Console.Clear();
        }

        //private static void WaitForFile(string filePath)
        //{
        //    Task.Run(() =>
        //    {
        //        bool fileInUse = true;
        //        while (fileInUse)
        //        {
        //            try
        //            {
        //                if (File.Exists(filePath))
        //                {
        //                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        //                    {
        //                        fileInUse = false;
        //                    }
        //                }
        //                else
        //                {
        //                    ConsoleExt.WriteLine($"{filePath} does not exist yet.");
        //                    fileInUse = false;
        //                }
        //            }
        //            catch (IOException)
        //            {
        //                fileInUse = true;
        //                ConsoleExt.WriteLine($"{filePath} is still in use.");
        //                Thread.Sleep(500);
        //            }
        //        }
        //    }).Wait(2500);
        //}

        public static void StreamProcessor()
        {
            while (true)
            {
                Capture = new FeedCapture();
                if (Capture.Main())
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }

        public static FeedCapture Capture;
        public static List<Thread> ClientThreads = new List<Thread>();

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
        //            ConsoleExt.WriteLine($"Shutdown caught in thread: {ex.Message}");
        //            File.AppendAllText($"{AssemblyDirectory}\\exception.log",
        //                $"{DateTime.Now:G} | Shutdown in {method.Method.Name}\r\n" +
        //                $"{ex.StackTrace}\r\n" +
        //                $"{ex.Source}\r\n" +
        //                $"{ex.Message}\r\n");
        //        }
        //        catch (Exception ex)
        //        {
        //            ConsoleExt.WriteLine($"Exception caught in thread: {ex.Message}");
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
            //try
            {
                while (true)
                {
                    //try
                    {
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.D0:
                                ConsoleExt.WriteLine("What do you want me to do? Divide by zero?");
                                break;
                            case ConsoleKey.D1:
                                ConsoleExt.WriteLine("--- SharpDataQueue ---");
                                foreach (SharpDataItem item in SharpDataQueue)
                                {
                                    ConsoleExt.WriteLine($"{item.Name}");
                                }
                                ConsoleExt.WriteLine("--- SharpDataHistory ---");
                                foreach (SharpDataItem item in SharpDataHistory)
                                {
                                    ConsoleExt.WriteLine($"{item.Name}");
                                }    
                                break;
                            case ConsoleKey.D2:
                                ConsoleExt.WriteLine(FriendlyVersion);
                                break;
                            case ConsoleKey.Escape:
                                new Thread(() => MainExt.UnsafeStateShutdown(KeyboardProcessor, new ArithmeticException(), "Crash initiated manually.", true)).Start();
                                break;
                            case ConsoleKey.Delete:
                                SharpDataHistory.Clear();
                                ConsoleExt.WriteLine("Cleared history list.");
                                break;
                            case ConsoleKey.Spacebar:
                                SkipPlayback = true;
                                ConsoleExt.WriteLine("Audio playback has been disabled for this time only.");
                                break;
                            //case ConsoleKey.G:
                            //    ConsoleExt.WriteLine("GUI shown.");
                            //    break;
                        }
                    }
                    Thread.Sleep(100);
                }
            }
        }

        public static void AddThread(ThreadStart method)
        {
            Thread thread = new Thread(() =>
            {
                try { method(); }
                catch (ThreadAbortException) { }
                catch (Exception e) { MainExt.UnsafeStateShutdown(null, e, e.Message); }
            });
            Program.MainThreads.Add((thread, method));
            thread.Start();
        }

        public static void RestartAllThreads()
        {
            foreach (var (thread, method) in Program.MainThreads)
            {
                RestartThread(thread, method);
            }
        }

        public static void RestartThread(Thread serviceThread, ThreadStart method)
        {
            if (serviceThread != null)
            {
                if (serviceThread.ThreadState != ThreadState.Stopped && serviceThread.ThreadState != ThreadState.Unstarted)
                    serviceThread.Abort();
            }

            Thread newThread = new Thread(method);
            newThread.Start();

            int index = Program.MainThreads.IndexOf((serviceThread, method));
            if (index >= 0)
            {
                Program.MainThreads[index] = (newThread, method);
            }
            else
            {
                Program.MainThreads.Add((newThread, method));
            }
        }

        public static Thread Watchdog()
        {
            return new Thread(() =>
            {
                string ProgramVersion = FriendlyVersion;
                Init(ProgramVersion, false);

                void ShutdownCapture()
                {
                    if (Capture != null)
                    {
                        Capture.ShutdownCapture = true;
                        while (Capture.ShutdownCapture)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    ConsoleExt.WriteLine($"{LanguageStrings.ThreadShutdown(Settings.Default.CurrentLanguage, $"Data Processor")}");
                }

                Config();

                while (true)
                {
                    Check.LastHeartbeat = DateTime.Now;

                    //if (!Program.MainThreads.Contains(Thread.CurrentThread)) Program.MainThreads.Add(Thread.CurrentThread);

                    AddThread(StreamProcessor);
                    AddThread(DataProcessor);
                    AddThread(AlertProcessor);
                    AddThread(KeyboardProcessor);

                    while (true)
                    {
                        if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 5)
                        {
                            if ((DateTime.Now - Check.LastHeartbeat).TotalMinutes >= 10)
                            {
                                ConsoleExt.WriteLine($"[Watchdog] {LanguageStrings.WatchdogForceRestartingProcess(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);

                                try
                                {
                                    ShutdownCapture();
                                    RestartAllThreads();
                                    Init(ProgramVersion, true);
                                    Thread.Sleep(5000);
                                }
                                catch (Exception)
                                {
                                    
                                }
                                
                                break;
                            }
                            //else ConsoleExt.WriteLine($"[Watchdog] {LanguageStrings.WatchdogForcedRestartWarning(Settings.Default.CurrentLanguage)}", ConsoleColor.Red);
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
        //internal static Thread StreamService;
        //internal static Thread RelayService;
        //internal static Thread KeyboardProc;
        //internal static Thread BatteryProc;
        internal static List<(Thread, ThreadStart)> MainThreads = new List<(Thread, ThreadStart)>();

        [STAThread]
        internal static void Main()
        {
            Console.Title = "SharpENDEC";
            Console.ForegroundColor = ConsoleColor.White;
            // PLUGIN IMPLEMENTATION!!!
            // BATTERY IMPLEMENTATION!!!
            // GUI IMPLEMENTATION!!!
            //Thread thread = new Thread(() => new ConsoleForm().ShowDialog());
            //thread.Start();
            WatchdogService = ENDEC.Watchdog();
            WatchdogService.Start();
            Console.CancelKeyPress += CancelAllOperations;
        }

        private static void CancelAllOperations(object sender, ConsoleCancelEventArgs e)
        {

        }
    }
}