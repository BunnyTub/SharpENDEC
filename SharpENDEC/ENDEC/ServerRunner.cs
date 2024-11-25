using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        internal static MainExt.RandomGenerator rnd = new MainExt.RandomGenerator();
        internal static List<string> AuthenticatedCookies = new List<string>();

        public static void HTTPServerProcessor()
        {
            lock (AuthenticatedCookies) AuthenticatedCookies.Clear();

            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8050/");
                listener.Start();

                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    Task.Run(() =>
                    {
                        string path = context.Request.Url.AbsolutePath.ToLower();
                        HttpListenerResponse response = context.Response;

                        List<string> QueryString = new List<string>();

                        foreach (string query in context.Request.QueryString)
                        {
                            QueryString.Add(query);
                        }

                        switch (path)
                        {
                            case "/":
                                Endpoints.HandleHomeEndpoint(response, context.Request.HttpMethod, QueryString);
                                break;
                            case "/version":
                                Endpoints.HandleVersionEndpoint(response, context.Request.HttpMethod, QueryString);
                                break;
                            case "/login":
                                Endpoints.HandleLoginEndpoint(response, context.Request.HttpMethod, QueryString);
                                break;
                            default:
                                HandleNotFound(response);
                                break;
                        }
                    });
                }
            }
            catch (Exception)
            {
                lock (AuthenticatedCookies) AuthenticatedCookies.Clear();
            }
        }

        internal static void WriteAndClose(HttpListenerResponse response, byte[] buffer, int code)
        {
            response.StatusCode = code;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private static class Endpoints
        {
            public static void HandleHomeEndpoint(HttpListenerResponse response, string method, List<string> query)
            {
                switch (method.ToUpper())
                {
                    case "GET":
                        byte[] buffer = Encoding.UTF8.GetBytes($"<html><body>The version is <a href=\"/version\">here</a>. Query: {query.ListToString()}</body></html>");
                        WriteAndClose(response, buffer, 200);
                        break;
                    default:
                        HandleWrongMethod(response);
                        break;
                }
            }
            
            public static void HandleHealthDataEndpoint(HttpListenerResponse response, string method, List<string> query)
            {
                switch (method.ToUpper())
                {
                    case "GET":
                        lock (CaptureThreads)
                        {
                            foreach (Thread thread in CaptureThreads)
                            {
                                if (thread.IsAlive) ConsoleExt.WriteLine(thread.Name + " is alive");
                            }
                        }
                        byte[] buffer = Encoding.UTF8.GetBytes("<html><body></body></html>");
                        WriteAndClose(response, buffer, 200);
                        break;
                    default:
                        HandleWrongMethod(response);
                        break;
                }
            }
            
            public static void HandleLoginEndpoint(HttpListenerResponse response, string method, List<string> query)
            {
                switch (method.ToUpper())
                {
                    case "GET":
                        byte[] buffer_GET = Encoding.UTF8.GetBytes($"<html><body>{rnd.Next(0, 10000)} {query}</body></html>");
                        WriteAndClose(response, buffer_GET, 200);
                        break;
                    case "POST":
                        byte[] buffer_POST = Encoding.UTF8.GetBytes($"<html><body>{rnd.Next(0, 10000)} {query}</body></html>");
                        AuthenticatedCookies.Add("");
                        response.SetCookie(new Cookie("Authentify", ""));
                        WriteAndClose(response, buffer_POST, 200);
                        break;
                    default:
                        HandleWrongMethod(response);
                        break;
                }
            }

            public static void HandleVersionEndpoint(HttpListenerResponse response, string method, List<string> query)
            {
                switch (method.ToUpper())
                {
                    case "GET":
                        byte[] buffer = Encoding.UTF8.GetBytes($"You queried \"{query.ListToString()}\"! Here's what you asked for though: {VersionInfo.FriendlyVersion}");
                        WriteAndClose(response, buffer, 200);
                        break;
                    default:
                        HandleWrongMethod(response);
                        break;
                }
            }
        }

        private static void HandleNotFound(HttpListenerResponse response)
        {
            string responseString = $"<html><title>{VersionInfo.FriendlyVersion}</title><body>Endpoint Not Found</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = 404;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        
        private static void HandleBadRequest(HttpListenerResponse response)
        {
            string responseString = $"<html><title>{VersionInfo.FriendlyVersion}</title><body>Bad Request</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = 400;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        
        private static void HandleUnauthorized(HttpListenerResponse response)
        {
            string responseString = $"<html><title>{VersionInfo.FriendlyVersion}</title><body>Request Unauthorized</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = 401;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        private static void HandleWrongMethod(HttpListenerResponse response)
        {
            string responseString = $"<html><title>{VersionInfo.FriendlyVersion}</title><body>Wrong Method</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = 405;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
