using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server.src
{
    public class MyHTTPRequest
    {
        public string? Method { get; set; }
        public string? Target { get; set; }
        public string? Version { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }

        public static MyHTTPRequest? ParseRequest(Socket socket)
        {
            string? requestString = Receive(socket);
            if (string.IsNullOrWhiteSpace(requestString)) return null;

            var requestParts = GetRequestParts(requestString);

            return new MyHTTPRequest
            {
                Method = requestParts.Method,
                Target = requestParts.Target,
                Version = requestParts.Version,
                Headers = GetHeaders(requestString),
                Body = GetBody(requestString)
            };
        }

        private static string? Receive(Socket socket)
        {
            string? data = "";

            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytesReceived = socket.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesReceived);

                if (bytesReceived <= 0 || socket.Available <= 0)
                {
                    break;
                }
            }

            return data;
        }

        private static (string Method, string Target, string Version) GetRequestParts(string requestString)
        {
            string requestLine = requestString.Substring(0, requestString.IndexOf("Host"));

            var requestParts = requestLine.Split(' ');

            return (requestParts[0].Trim(), requestParts[1].Trim(), requestParts[2].Trim());
        }
        private static Dictionary<string, string> GetHeaders(string requestString)
        {
           string strHeaders = requestString.Substring(requestString.IndexOf("Host"), requestString.IndexOf("\r\n\r\n") - requestString.IndexOf("Host"));

           string[] headers = strHeaders.Split("\r\n");

           Dictionary<string, string> dicHeaders = new Dictionary<string, string>();

           foreach( var header in headers) 
           {
            var keyValues = header.Split(':');
            dicHeaders.Add(keyValues[0].Trim(), keyValues[1].Trim());
           }

           return dicHeaders;
        }

        private static string GetBody(string requestString)
        {
           return requestString.Substring(requestString.IndexOf("\r\n\r\n") + 2).Trim();
        }
    }
}