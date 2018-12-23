using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebSock.Server
{
    class Program
    {
        static void Main()
        {

            var server = new HttpSimpleServer(6543);

            server.Handle("test.html",(path, s) =>
            {
                var r = new StreamWriter(s);
                Console.WriteLine($"PATH:{path}"); 
                r.WriteLine("HTTP/1.1 200 OK");
                r.WriteLine("Content-Type: text/html");
                r.WriteLine();
                for (int i = 0; i < 10; i++)
                {
                    r.WriteLine("<h1>hi<h1>");
                    r.Flush();
                    Thread.Sleep(1000);
                }
            });

            server.Handle("index.html",(path, s) => WriteResource("index.html", s));
            server.Handle("tachyons.css",(path, s) => WriteResource("tachyons.css", s));
            server.Handle("main.js",(path, s) => WriteResource("main.js", s));

            server.Start();
            
            Console.ReadLine();
        }

        private static void WriteResource(string name, Stream s)
        {
            string type = "text/html";
            if (name.EndsWith(".css")) type = "text/css";
            if (name.EndsWith(".js")) type = "text/javascript";
            var r = new StreamWriter(s);
            Console.WriteLine($"RESOURCE:{name}"); 
            r.WriteLine("HTTP/1.1 200 OK");
            r.WriteLine($"Content-Type: {type}");
            r.WriteLine();
            r.Flush();
            Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(typeof(Program), name)
                .CopyTo(s);
        }


        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
        static void MainWebScoket(string[] args)
        {
            Console.WriteLine("Hello World!");
            var tcpListener = new TcpListener(IPAddress.Any, 6543);
            tcpListener.Start();
            Console.WriteLine("Started at 0.0.0.0:6543");

            var client = tcpListener.AcceptTcpClient();
            
            Console.WriteLine("Connected");

            var stream = client.GetStream();
            while (true)
            {
                while (!stream.DataAvailable)
                {
                    
                }

                while (client.Available < 3)
                {
                    
                }

                var bytes = new byte[client.Available];
                bytes.GetHashCode();
                
                stream.Read(bytes, 0, bytes.Length);

                var data = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(data, "^GET"))
                {
                    const string eol = "\r\n";
                    var key = Regex.Match("Sec-WebSocket-Key: (.*)", data).Groups[1].Value.Trim();
                    var keyBuffer = Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
                    var hashBuffer = SHA1.Create().ComputeHash(keyBuffer);
                    var hash = Convert.ToBase64String(hashBuffer);

                    var head = $"HTTP/1.1 101 Switching Protocols{eol}"
                                + $"Connection: Upgrade{eol}"
                                + $"Upgrade: websocket{eol}"
                                + $"Sec-WebSocket-Accept: {hash}{eol}"
                                + $"{eol}";
                    
                    var response = Encoding.UTF8.GetBytes(head);
                }
            }
        }
    }
}