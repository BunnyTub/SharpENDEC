using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ThreadState = System.Threading.ThreadState;

namespace SharpENDEC
{
    public static class MainExt
    {
        private static Exception ex = new Exception();

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
            return new Exception("DummyException");
        }

        /// <summary>
        /// Immediately shuts down all threads, writes the exception to the console and disk, then waits infinitely.
        /// </summary>
        /// <param name="caller">The method that called the exception.</param>
        /// <param name="exception">The exception that caused the unsafe state.</param>
        /// <param name="reason">The reason why the unsafe state was invoked.</param>
        /// <param name="restart">Whether or not to restart and ignore the infinite rule.</param>
        public static void UnsafeStateShutdown(Action caller, Exception exception, string reason, bool restart = true)
        {
            lock (ex)
            {
                if (caller.IsNull()) caller = UnknownCaller;
                if (exception.IsNull()) exception = UnknownException();
                if (string.IsNullOrEmpty(reason)) reason = "No reason provided.";

                ex = exception;

                lock (ENDEC.ClientThreads)
                {
                    foreach (Thread thread in ENDEC.ClientThreads)
                        try { if (!thread.IsNull()) thread.Abort(ex); } catch (Exception) { }
                    ENDEC.ClientThreads.Clear();
                }

                lock (Program.MainThreads)
                {
                    foreach (var (thread, method) in Program.MainThreads)
                        try { if (!thread.IsNull()) thread.Abort(ex); } catch (Exception) { }
                    Program.MainThreads.Clear();
                }

                ENDEC.Capture = null;

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                string full = ($"{DateTime.Now:G} | Called by {caller.Method.Name}\r\n" +
                    $"Stack Trace: {ex.StackTrace}\r\n" +
                    $"Source: {ex.Source}\r\n" +
                    $"Message: {ex.Message}\r\n" +
                    $"{reason}");
                Console.WriteLine(full);
                Thread.Sleep(1000);
                Debug.Assert(false, "The program went into an unsafe state.", full);
                Console.Clear();
                if (restart)
                {
                    Console.Clear();
                    new Thread(() => Program.Main()).Start();
                }
                else Thread.Sleep(Timeout.Infinite);

            }
        }

        public static bool IsNull(this object obj)
        {
            if (obj == null) return true;
            return false;
        }
    }
}
