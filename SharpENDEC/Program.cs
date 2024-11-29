using NAudio.Wave;
using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using static SharpENDEC.VersionInfo;
using ThreadState = System.Threading.ThreadState;

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
            EnablePerformanceProcessor = false;
            Console.Title = VersionID + " | *";
            EnablePerformanceProcessor = true;

            ConsoleExt.WriteLine($"{VersionID}\r\n" +
                $"This project wouldn't have been possible without ApatheticDELL's QuantumENDEC!\r\n\r\n" +
                $"Project created by BunnyTub.\r\n" +
                $"Logo created by ApatheticDELL.\r\n" +
                $"Translations may not be 100% accurate due to language deviations.\r\n" +
                $"Translations are not fully complete, and may be undone.\r\n");

            if (RecoveredFromProblem) ConsoleExt.WriteLine(LanguageStrings.RecoveredFromFailure(Settings.Default.CurrentLanguage), ConsoleColor.DarkRed);
            if (IsAdministrator) ConsoleExt.WriteLine(LanguageStrings.ElevationSecurityProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);
            if (IsGuest) ConsoleExt.WriteLine(LanguageStrings.ConfigurationLossProblem(Settings.Default.CurrentLanguage), ConsoleColor.Yellow);

            CheckFolder("Audio", false);

            if (!File.Exists($"{AudioDirectory}\\attn.wav"))
            {
                ConsoleExt.WriteLine("The attention tone audio \"attn.wav\" doesn't exist. The default one will be extracted and used instead.");
                MemoryStream mem = new MemoryStream();
                Resources.attn.CopyTo(mem);
                File.WriteAllBytes("Audio\\attn.wav", mem.ToArray());
                mem.Dispose();
            }

            //ConsoleExt.WriteLine();
            ConsoleExt.WriteLine($"Press SPACE to pause for 15 seconds.");

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
                            ConsoleExt.WriteLine("Paused for 15 seconds.");
                            Thread.Sleep(15000);
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
                                ConsoleExt.WriteLine($"2. Français | Use the program in French");
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
        public static List<Thread> CaptureThreads = new List<Thread>();

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
                            case ConsoleKey.O:
                                OpenFileDialog AlertFileDialog = new OpenFileDialog
                                {
                                    //InitialDirectory = "C:\\",
                                    Filter = "CAP files (*.xml, *.cap)|*.xml;*.cap",
                                    FilterIndex = 0,
                                    CheckFileExists = true,
                                    Multiselect = false
                                };
                                //AlertFileDialog.RestoreDirectory = true;

                                ConsoleExt.WriteLine("Opening file picker.");

                                if (AlertFileDialog.ShowDialog() != DialogResult.OK)
                                {
                                    ConsoleExt.WriteLine("No file chosen.");
                                    break;
                                }

                                try
                                {
                                    string data = File.ReadAllText(AlertFileDialog.FileName);
                                    lock (SharpDataQueue)
                                        SharpDataQueue.Add(new SharpDataItem(AlertFileDialog.SafeFileName, data));
                                    ConsoleExt.WriteLine("Added file to queue.");
                                }
                                catch (Exception e)
                                {
                                    ConsoleExt.WriteLineErr(e.Message);
                                }
                                finally
                                {
                                    AlertFileDialog.Dispose();
                                }

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

        public static bool EnablePerformanceProcessor = false;

        public static void TitleProcessor()
        {
            string originalTitle = Console.Title;
            Process thisProcess = Process.GetCurrentProcess();
            PerformanceCounter cpuCounter;
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            while (true)
            {
                if (EnablePerformanceProcessor)
                {
                    Console.Title = originalTitle.Replace("*", $"{cpuCounter.NextValue() / Environment.ProcessorCount:0.00}% CPU - {thisProcess.WorkingSet64 / (1024 * 1024)} MB");
                    thisProcess.Refresh();
                }
                Thread.Sleep(1000);
            }
        }

        public static void BatteryProcessor()
        {
            // if (Battery.UsageEnabled) then DoBatteryStuff
            //try
            {
                while (true)
                {
                    try
                    {
                        foreach (ManagementObject battery in new ManagementObjectSearcher("Select * from Win32_Battery")
                            .Get()
                            .Cast<ManagementObject>())
                        {
                            string name = battery["Name"]?.ToString();
                            string status = battery["Status"]?.ToString();
                            int estimatedChargeRemaining = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                            int estimatedRunTime = Convert.ToInt32(battery["EstimatedRunTime"]);

                            Console.WriteLine($"Battery Name: {name}");
                            Console.WriteLine($"Status: {status}");
                            Console.WriteLine($"Charge Remaining: {estimatedChargeRemaining}%");

                            // Estimated run time in minutes, -1 means unknown
                            if (estimatedRunTime != -1)
                            {
                                Console.WriteLine($"Estimated Run Time: {TimeSpan.FromMinutes(estimatedRunTime)}");
                            }
                            else
                            {
                                Console.WriteLine("Estimated Run Time: Unknown");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                    //Thread.Sleep(10000);
                    Thread.Sleep(1000);
                }
            }
        }

        public static Thread AddThread(ThreadStart method, bool isMTA)
        {
            Thread thread = new Thread(() =>
            {
                try { method(); }
                catch (ThreadAbortException) { }
                catch (Exception e) { MainExt.UnsafeStateShutdown(null, e, e.Message); }
            });
            if (isMTA) thread.SetApartmentState(ApartmentState.MTA);
            else thread.SetApartmentState(ApartmentState.STA);
            Program.MainThreads.Add((thread, method, isMTA));
            thread.Start();
            return thread;
        }

        public static void KillAllThreads()
        {
            var threadList = new List<Thread>();
            foreach (var (thread, _, _) in Program.MainThreads)
            {
                threadList.Add(thread);
            }
            Program.MainThreads.Clear();
            foreach (var thread in threadList)
            {
                thread.Abort();
            }
        }
        
        public static void RestartAllThreads()
        {
            var threadList = new List<(Thread thread, ThreadStart method, bool isMTA)>();
            foreach (var (thread, method, isMTA) in Program.MainThreads)
            {
                threadList.Add((thread, method, isMTA));
                RestartThread(thread, method, isMTA);
            }
            Program.MainThreads.Clear();
            foreach (var (thread, method, isMTA) in threadList)
            {
                Program.MainThreads.Add((thread, method, isMTA));
            }
        }

        public static void RestartThread(Thread serviceThread, ThreadStart method, bool IsMTAThread)
        {
            if (serviceThread != null)
            {
                if (serviceThread.ThreadState != ThreadState.Stopped && serviceThread.ThreadState != ThreadState.Unstarted)
                    serviceThread.Abort();
            }

            Thread thread = new Thread(() =>
            {
                try { method(); }
                catch (ThreadAbortException) { }
                catch (Exception e) { MainExt.UnsafeStateShutdown(null, e, e.Message); }
            });
            if (IsMTAThread) thread.SetApartmentState(ApartmentState.MTA);
            else thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            //int index = Program.MainThreads.IndexOf((serviceThread, method));
            //if (index >= 0)
            //{
            //    Program.MainThreads[index] = (newThread, method);
            //}
            //else
            //{
            //    Program.MainThreads.Add((newThread, method));
            //}
        }

        //public static IEnumerable<Type> handlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
        //        .Where(p => typeof(ISharpPlugin).IsAssignableFrom(p) && p.IsClass);

        public static Thread Watchdog()
        {
            return new Thread(() =>
            {
                try
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

                    //foreach (var handler in handlers)
                    //{
                    //    var handlerInstance = (ISharpPlugin)Activator.CreateInstance(handler);
                    //    //handlerInstance.AlertBlacklisted();
                    //}

#if !DEBUG
                    Process parent;

                    try
                    {
                        var myId = Process.GetCurrentProcess().Id;
                        var search = new ManagementObjectSearcher("root\\CIMV2",
                            $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {Process.GetCurrentProcess().Id}");
                        var results = search.Get().GetEnumerator();
                        results.MoveNext();
                        parent = Process.GetProcessById((int)(uint)results.Current["ParentProcessId"]);
                    }
                    catch (Exception)
                    {
                        ConsoleExt.WriteLine("Process is malfunctioning.");
                        return;
                    }
#endif

                    //if (!Program.MainThreads.Contains(Thread.CurrentThread)) Program.MainThreads.Add(Thread.CurrentThread);

                    AddThread(StreamProcessor, true);
                    AddThread(DataProcessor, true);
                    AddThread(AlertProcessor, true);
                    AddThread(KeyboardProcessor, false);
                    AddThread(TitleProcessor, true);
                    //AddThread(BatteryProcessor);
                    AddThread(HTTPServerProcessor, true);

                    Check.LastHeartbeat = DateTime.Now;

                    while (true)
                    {
#if !DEBUG
                        if (parent.HasExited)
                        {
                            ShutdownCapture();
                            KillAllThreads();
                            Environment.Exit(1985);
                        }
#endif

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
                catch (ThreadAbortException) { }
                catch (Exception e) { MainExt.UnsafeStateShutdown(null, e, e.Message); }
            });
        }
    }

    internal static class Program
    {
        internal static Thread WatchdogService;
        internal static List<(Thread, ThreadStart, bool)> MainThreads = new List<(Thread, ThreadStart, bool)>();
        internal const uint ENABLE_QUICK_EDIT = 0x0040;
        internal const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        public const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [STAThread]
        internal static void Main(string[] args)
        {
            Console.Title = "SharpENDEC";
            Console.ForegroundColor = ConsoleColor.White;

#if !DEBUG
            string WatchdogArgument = "--redundant-watchdog";

            if (args.Length == 1)
            {
                if (args[0] == WatchdogArgument)
                {
#endif
                    DisableConsoleHighlighting();
                    // BATTERY IMPLEMENTATION!!! - BUSY
                    // GUI IMPLEMENTATION!!! - UNKNOWN / BUSY
                    WatchdogService = ENDEC.Watchdog();
                    WatchdogService.Start();
#if !DEBUG
                }
            }
            else
            {
                bool restart = false;

                do
                {
                    using (var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
                        {
                            Arguments = WatchdogArgument,
                            UseShellExecute = false
                        }
                    })
                    {
                        p.Start();
                        p.WaitForExit();

                        if (p.ExitCode == 1984)
                        {
                            restart = true;
                        }
                        else
                        {
                            restart = false;
                        }
                    }
                } while (restart);
            }
#endif
        }

        internal static bool DisableConsoleHighlighting()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            if (!GetConsoleMode(consoleHandle, out uint mode))
            {
                var MarshalException = new Win32Exception(unchecked(Marshal.GetLastWin32Error()));
                ConsoleExt.WriteLineErr($"Could not get the console mode. This may cause problems if you select text often.\r\n" +
                    $"You can ignore this message if you're not running Windows 10 or above.\r\n" +
                    $"Reason: {MarshalException.Message}");
                Thread.Sleep(5000);
                return false;
            }

            mode &= ~ENABLE_QUICK_EDIT;
            mode |= ENABLE_EXTENDED_FLAGS;

            if (!SetConsoleMode(consoleHandle, mode))
            {
                var MarshalException = new Win32Exception(unchecked(Marshal.GetLastWin32Error()));
                ConsoleExt.WriteLineErr($"Could not set the console mode. This may cause problems if you select text often.\r\n" +
                    $"You can ignore this message if you're not running Windows 10 or above.\r\n" +
                    $"Reason: {MarshalException.Message}");
                Thread.Sleep(5000);
                return false;
            }

            // if we don't use unchecked(), the error might cause an error
            return true;
        }
    }
}