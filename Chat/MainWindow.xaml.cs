using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        TcpClient client;
        string Username = string.Empty;

        void ReceiveData(TcpClient client)
        {
            while (true)
            {
                var stream = client.GetStream();

                var buffer = new byte[1024];
                var byteCount = stream.Read(buffer, 0, buffer.Length);

                if (byteCount > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    data = data.Replace("\0", string.Empty);

                    var messageParts = data.Split('|');

                    switch (messageParts[0])
                    {
                        case "message":
                            Dispatcher.Invoke(() =>
                            {
                                ChatText.Text = messageParts[1] + ": " + messageParts[2] + Environment.NewLine + ChatText.Text;
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Username = NameInput.Text;

                var ipAddress = IPAddress.Parse(IpAddressInput.Text);
                client = new TcpClient();
                client.Connect(ipAddress, 5000);
                SendButton.IsEnabled = true;

                SendData(string.Format("connect|{0}", Username));

                ChatText.Text = string.Empty;

                var thread = new Thread(() => ReceiveData(client));
                thread.Start();
            }
            catch (FormatException)
            {
                MessageBox.Show("Ungültige IP-Adresse");
            }
            catch(SocketException)
            {
                MessageBox.Show("Server nicht erreichbar");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var messageText = string.Format("message|{0}|{1}", Username, MessageInput.Text);
            MessageInput.Text = string.Empty;
            SendData(messageText);
        }

        private void SendData(string messageText)
        {
            var stream = client.GetStream();
            var messageTextBytes = Encoding.UTF8.GetBytes(messageText);
            stream.Write(messageTextBytes, 0, messageTextBytes.Length);
        }
    }
}
