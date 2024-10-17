using SharpENDEC.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
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
                        Console.WriteLine($"[{host}:{port}] {SVDictionary.ConnectedToServer(Settings.Default.CurrentLanguage, host, port)}");

                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE
                        //while (true) Check.LastHeartbeat = DateTime.Now;
                        // FOR DEBUGGING --- REMOVE --- FOR DEBUGGING --- REMOVE

                        string dataReceived = string.Empty;

                        List<byte> data = new List<byte>();

                        while (true)
                        {
                            if (ShutdownCapture)
                            {
                                Console.WriteLine($"{SVDictionary.ThreadShutdown(Settings.Default.CurrentLanguage, $"Stream Processing ({host}:{port})")}");
                            }
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
                                    // The Math.Pow is here to intentionally slow down the TCP stream download,
                                    // because it's simply too fast, and we can sometimes continue early without it.
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
                    Console.WriteLine($"[{host}:{port}] {SVDictionary.HostTimedOut(Settings.Default.CurrentLanguage, host)}");
                    return;
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine($"{SVDictionary.ThreadShutdown(Settings.Default.CurrentLanguage, $"Stream Processing ({host}:{port})")}");
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
                    Console.WriteLine($"{SVDictionary.StartingConnection(Settings.Default.CurrentLanguage, server, 8080)}");
                }

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
                                    Console.WriteLine($"{SVDictionary.RestartingAfterException(Settings.Default.CurrentLanguage)}");
                                    string server = Settings.Default.CanadianServers[i];
                                    Thread newThread = new Thread(() => Receive(server, 8080, "</alert>"));
                                    newThread.Start();
                                    ClientThreads[i] = newThread;
                                }
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
    }
}
