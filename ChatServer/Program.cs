using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static List<TcpClient> Clients = new List<TcpClient>();
        static bool ParallelExecution = false;
        static Timer SendClientUpdateTimer;
        static List<string> Usernames = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Soll der Server parallel verarbeiten? (y/n)");
            var input = Console.ReadKey();
            Console.WriteLine();

            if (input.Key == ConsoleKey.Y)
            {
                Console.WriteLine("Parallele Verarbeitung aktiviert.");
                ParallelExecution = true;
            }
            else
            {
                Console.WriteLine("Parallele Verarbeitung deaktiviert.");
            }

            var tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();

            SendClientUpdateTimer = new Timer(SendClientUpdate, null, 0, 1000);

            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                Clients.Add(client);

                var thread = new Thread(() => HandleClient(client));
                thread.Start();

                Console.WriteLine("Verbunden: {0}", client.Client.RemoteEndPoint);
                Console.WriteLine("Es sind {0} Teilnehmer online.", Clients.Count);
            }
        }

        private static void SendClientUpdate(object state)
        {
            string message = string.Format("Es sind {0} Teilnehmer online", Clients.Count);
            var byteMessage = Encoding.UTF8.GetBytes(message);
            Broadcast(byteMessage);
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
                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine(data);

                    var messageParts = data.Split('|');

                    switch (messageParts[0])
                    {
                        case "message":
                            Broadcast(buffer);
                            break;
                        case "connect":
                            Usernames.Add(messageParts[1]);
                            string userListMessage = string.Format("user_list|{0}", string.Join(",", Usernames));
                            var userListMessageBuffer = Encoding.UTF8.GetBytes(userListMessage);
                            Broadcast(userListMessageBuffer);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void Broadcast(byte[] buffer)
        {
            if (ParallelExecution)
            {
                Parallel.ForEach(Clients, otherClient =>
                {
                    var otherStream = otherClient.GetStream();
                    otherStream.Write(buffer, 0, buffer.Length);
                });
            }
            else
            {
                foreach (var otherClient in Clients)
                {
                    var otherStream = otherClient.GetStream();
                    otherStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
