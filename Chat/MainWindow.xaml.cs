using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ipAddress = IPAddress.Parse(IpAddressInput.Text);
                client = new TcpClient();
                client.Connect(ipAddress, 5000);
                SendButton.IsEnabled = true;
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
            var messageText = MessageInput.Text;
            MessageInput.Text = string.Empty;

            var stream = client.GetStream();

            var messageTextBytes = Encoding.ASCII.GetBytes(messageText);
            stream.Write(messageTextBytes, 0, messageTextBytes.Length);
        }
    }
}
