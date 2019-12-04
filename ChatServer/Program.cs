using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static List<TcpClient> Clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            //var tcpListener = new TcpListener(5000); <- obsolete
            var tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();

            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                Clients.Add(client);

                var thread = new Thread(() => HandleClient(client));
                thread.Start();

                Console.WriteLine("Es sind {0} Teilnehmer online.", Clients.Count);
            }
        }

        static void HandleClient(TcpClient client)
        {
            while (true)
            {
                var stream = client.GetStream();

                var buffer = new byte[1024];
                var byteCount = stream.Read(buffer, 0, buffer.Length);

                if (byteCount > 0)
                {
                    string data = Encoding.ASCII.GetString(buffer, 0, byteCount);
                    Console.WriteLine(data);
                }
            }
        }
    }
}
