using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace SharpENDEC
{
    public static class MainExt
    {
        public static Exception LastException { get; private set; }

        /// <summary>
        /// This method is for dummy purposes, and does not perform any tasks.
        /// </summary>
        public static void UnknownCaller()
        {
        }

        /// <summary>
        /// This method is for dummy purposes, and does not perform any tasks.
        /// </summary>
        public static Exception UnknownException()
        {
            return new Exception("UnknownException");
        }

        /// <summary>
        /// Immediately shuts down all threads, writes the exception to the console and disk, then waits infinitely.
        /// </summary>
        /// <param name="caller">The method that called the exception.</param>
        /// <param name="exception">The exception that caused the unsafe state.</param>
        /// <param name="reason">The reason why the unsafe state was invoked.</param>
        /// <param name="restart">Whether or not to restart and ignore the infinite wait rule.</param>
        public static void UnsafeStateShutdown(Action caller, Exception exception, string reason, bool restart = true)
        {
            LastException = exception;
            lock (LastException)
            {
                if (caller.IsNull()) caller = UnknownCaller;
                if (exception.IsNull()) exception = UnknownException();
                if (string.IsNullOrEmpty(reason)) reason = "No reason provided.";

                try { if (!Program.WatchdogService.IsNull()) Program.WatchdogService.Abort(LastException); } catch (Exception) { }

                DateTime ExceptionTime = DateTime.Now;

                lock (ENDEC.CaptureThreads)
                {
                    foreach (Thread thread in ENDEC.CaptureThreads)
                        try { if (!thread.IsNull()) thread.Abort(LastException); } catch (Exception) { }
                    ENDEC.CaptureThreads.Clear();
                }

                lock (Program.MainThreads)
                {
                    foreach (var (thread, method, isMTA) in Program.MainThreads)
                        try { if (!thread.IsNull()) thread.Abort(LastException); } catch (Exception) { }
                    Program.MainThreads.Clear();
                }

                ENDEC.Capture = null;
                ENDEC.SharpDataQueue = null;
                ENDEC.SharpDataHistory = null;

                Thread.Sleep(100);

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                string full = $"{ExceptionTime:G} | Called by {caller.Method.Name}\r\n" +
                    $"Exception Stack Trace: {LastException.StackTrace}\r\n" +
                    $"Exception Source: {LastException.Source}\r\n" +
                    $"Exception Message: {LastException.Message}\r\n" +
                    $"Caller Reason: {reason}";
                Console.WriteLine(full);
                try
                {
                    string ExceptionDateTime = ExceptionTime.ToString("s").Replace(":", "-");
                    File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\exception_{ExceptionDateTime}.txt", full + "\r\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Another exception occurred while trying to write an exception.\r\n{e.Message}");
                }
#if DEBUG
                Debug.Assert(false, "The program went into an unsafe state.", full);
#else
                Thread.Sleep(5000);
#endif
                if (restart)
                {
                    Console.Clear();
#if !DEBUG
                    Environment.Exit(1984);
#else
                    new Thread(() => Program.Main(null)).Start();
#endif
                }
                else
                {
                    Environment.Exit(1985);
                }
            }
        }

        public static bool IsNull(this object obj)
        {
            if (obj == null) return true;
            return false;
        }
        
        public static string ListToString(this List<string> obj)
        {
            if (obj == null) return string.Empty;
            if (obj.Count == 0) return string.Empty;
            string all = string.Empty;
            foreach (string str in obj)
            {
                all += $"{str},\x20";
            }
            return all.Substring(all.Length, all.Length - 2);
        }

        public class RandomGenerator
        {
            readonly RNGCryptoServiceProvider csp;

            public RandomGenerator()
            {
                csp = new RNGCryptoServiceProvider();
            }

            public int Next(int minValue, int maxExclusiveValue)
            {
                if (minValue >= maxExclusiveValue)
                    throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

                long diff = (long)maxExclusiveValue - minValue;
                long upperBound = uint.MaxValue / diff * diff;

                uint ui;
                do
                {
                    ui = GetRandomUInt();
                } while (ui >= upperBound);
                return (int)(minValue + (ui % diff));
            }

            private uint GetRandomUInt()
            {
                var randomBytes = GenerateRandomBytes(sizeof(uint));
                return BitConverter.ToUInt32(randomBytes, 0);
            }

            private byte[] GenerateRandomBytes(int bytesNumber)
            {
                byte[] buffer = new byte[bytesNumber];
                csp.GetBytes(buffer);
                return buffer;
            }
        }
    }
}
