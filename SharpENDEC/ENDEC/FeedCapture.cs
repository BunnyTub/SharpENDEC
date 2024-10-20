using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        public static List<SharpDataItem> SharpDataQueue = new List<SharpDataItem>();
        public static List<SharpDataItem> SharpDataHistory = new List<SharpDataItem>();

        public class SharpDataItem
        {
            public string Name { get; set; }
            public string Data { get; set; }

            public SharpDataItem(string name, string data)
            {
                Name = name;
                Data = data;
            }
        }

        public class FeedCapture
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
                        ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.ConnectedToServer(Settings.Default.CurrentLanguage, host, port)}", ConsoleColor.Green);

                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE
                        //while (true) Check.LastHeartbeat = DateTime.Now;
                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE

                        string dataReceived = string.Empty;

                        List<byte> data = new List<byte>();

                        while (true)
                        {
                            if (ShutdownCapture)
                            {
                                ConsoleExt.WriteLine($"{LanguageStrings.ThreadShutdown(Settings.Default.CurrentLanguage, $"Stream Processing ({host}:{port})")}");
                            }
                            while (!stream.DataAvailable)
                            {
                                try
                                {
                                    stream.WriteByte(0);
                                }
                                catch (IOException e)
                                {
                                    ConsoleExt.WriteLine($"[{host}:{port}] {e.Message}", ConsoleColor.Red);
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            data.Clear();
                            DateTime now = DateTime.Now;
                            ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.ProcessingStream(Settings.Default.CurrentLanguage)}");

                            while (stream.DataAvailable)
                            {
                                int bit = stream.ReadByte();
                                if (bit != -1)
                                {
                                    data.Add((byte)bit);
                                    // The Math.Pow is here to intentionally slow down the TCP stream download,
                                    // because it's simply too fast, and we can sometimes continue early without it.
                                    // Although we may not need it.
                                    //Math.Pow(bit, bit);
                                }
                            }

                            string chunk = Encoding.UTF8.GetString(data.ToArray(), 0, data.Count);

                            dataReceived += chunk;

                            if (chunk.Contains(delimiter))
                            {
                                ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.ProcessedStream(Settings.Default.CurrentLanguage, data.Count, now)}");
                                string capturedSent = Regex.Match(dataReceived, @"<sent>\s*(.*?)\s*</sent>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value.Replace("-", "_").Replace("+", "p").Replace(":", "_");
                                string capturedIdent = Regex.Match(dataReceived, @"<identifier>\s*(.*?)\s*</identifier>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value.Replace("-", "_").Replace("+", "p").Replace(":", "_");
                                string filename = $"{capturedSent}I{capturedIdent}.xml";

                                if (SharpDataQueue.Any(x => x.Name == filename) || SharpDataHistory.Any(x => x.Name == filename))
                                {
                                    ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.DataPreviouslyProcessed(Settings.Default.CurrentLanguage)}", ConsoleColor.DarkGray);
                                }
                                else
                                {
                                    SharpDataQueue.Add(new SharpDataItem(filename, dataReceived));
                                    ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.FileDownloaded(Settings.Default.CurrentLanguage, host)}");
                                }
                                dataReceived = string.Empty;
                            }
                            else
                            {
                                if (data.Count > 10000000)
                                {
                                    throw new Exception($"[{host}:{port}] The data exceeds the 10 MB limit. The server may be malfunctioning.");
                                }
                                else ConsoleExt.WriteLine($"[{host}:{port}] {data.Count} bytes total including the current chunk.");
                            }
                        }
                    }
                }
                catch (SocketException e)
                {
                    ConsoleExt.WriteLine($"[{host}:{port}] {e.Message}");
                    Thread.Sleep(1000);
                    return;
                }
                catch (TimeoutException)
                {
                    ConsoleExt.WriteLine($"[{host}:{port}] {LanguageStrings.HostTimedOut(Settings.Default.CurrentLanguage, host)}");
                    return;
                }
                catch (ThreadAbortException)
                {
                    ShutdownCapture = true;
                    //ConsoleExt.WriteLine($"{LanguageStrings.ThreadShutdown(Settings.Default.CurrentLanguage, $"Stream Processing ({host}:{port})")}");
                    return;
                }
            }

            public bool Main()
            {
                SharpDataQueue = new List<SharpDataItem>();
                SharpDataHistory = new List<SharpDataItem>();

                void StartServerConnection()
                {
                    foreach (string server in Settings.Default.CanadianServers)
                    {
                        Thread thread = new Thread(() => Receive(server, 8080, "</alert>"));
                        thread.Start();
                        ClientThreads.Add(thread);
                        ConsoleExt.WriteLine($"{LanguageStrings.StartingConnection(Settings.Default.CurrentLanguage, server, 8080)}", ConsoleColor.DarkGray);
                        Thread.Sleep(250);
                    }
                }

                StartServerConnection();

                try
                {
                    while (true)
                    {
                        for (int i = 0; i < ClientThreads.Count; i++)
                        {
                            if (!ClientThreads[i].IsAlive)
                            {
                                if (!ShutdownCapture)
                                {
                                    ConsoleExt.WriteLine($"{LanguageStrings.RestartingAfterException(Settings.Default.CurrentLanguage)}");
                                    string server = Settings.Default.CanadianServers[i];
                                    Thread newThread = new Thread(() => Receive(server, 8080, "</alert>"));
                                    newThread.Start();
                                    ClientThreads[i] = newThread;
                                }
                            }
                        }

                        if (ShutdownCapture)
                        {
                            lock (ClientThreads)
                            foreach (var thread in ClientThreads)
                            {
                                try
                                {
                                    if (thread.IsAlive) thread.Join();
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
    }
}
