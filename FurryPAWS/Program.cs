using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FurryPAWS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:64/");
            listener.Start();
            Console.WriteLine("Listening for incoming connections on http://*:64/");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            }
        }

        static void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString;

            if (File.Exists($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\file.xml"))
            {
                responseString = File.ReadAllText($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\file.xml");
            }
            else
            {
                responseString = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><ns1:alerts xmlns:ns1=\"http://cast.bunnytub.com:8181/\"></ns1:alerts>";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            using (Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }

            Console.WriteLine($"Request from {request.RemoteEndPoint} handled.");
        }
    }
}
