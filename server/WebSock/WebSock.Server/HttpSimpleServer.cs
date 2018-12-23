using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSock.Server
{
    class HttpSimpleServer
    {
        private readonly int _port;

        readonly Dictionary<string, Action<string, Stream>> _handlers = new Dictionary<string, Action<string, Stream>>();
        public HttpSimpleServer(int port)
        {
            _port = port;
        }

        public void Handle(string path, Action<string, Stream> handler)
        {
            _handlers.Add(path, handler);
        }
        
        public void Start()
        {
            var tcpListener = new TcpListener(IPAddress.Any, _port);
            tcpListener.Start();
            Console.WriteLine("Started at 0.0.0.0:6543");
            Task.Run(() =>
            {
                try
                {
                    Accept(tcpListener);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private void Accept(TcpListener tcpListener)
        {
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                Console.WriteLine("Connected");
                Task.Run(() =>
                {
                    try
                    {
                        ClientRun(client);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }

        private void ClientRun(TcpClient client)
        {
            var stream = client.GetStream();

            var sr = new StreamReader(stream);
            string url = null;
            string method = null;
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) return;
                if (line == "") break;
                // Console.WriteLine(line);
                if (url == null)
                {
                    var m = Regex.Match(line, @"^(\S+)\s+(\S+)");
                    if (m.Success)
                    {
                        method = m.Groups[1].Value;
                        url = m.Groups[2].Value;
                        Console.WriteLine($"{method} {url}");
                    }
                }
            }

            if (url == null) return;
            
            var path = Regex.Match(url, @"^([^\?]+)").Groups[1].Value;
            if (path == "/") path = "index.html";
            Console.WriteLine($"PATH:{path}"); 
            
            foreach (var pair in _handlers)
            {
                var pathKey = pair.Key;
                var handler = pair.Value;
                if (path.EndsWith(pathKey))
                {
                    handler(url, stream);
                    client.Close();
                    return;
                }
            }
            var r = new StreamWriter(stream);
            r.WriteLine("HTTP/1.1 404 NOT FOUND");
            r.WriteLine("Content-Type: text/html");
            r.WriteLine();
            r.WriteLine("Not found");
            client.Close();
        }
    }
}
