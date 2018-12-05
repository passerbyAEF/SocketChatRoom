using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;

namespace Sever
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener tcp;
        List<Socket> sockets;
        byte[] b = new byte[1024 * 500];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(IPBox.Text) | string.IsNullOrWhiteSpace(IPBox.Text))
            {
                IPAddress[] ipa = Dns.GetHostAddresses(Dns.GetHostName());
                tcp = new TcpListener(ipa[ipa.Length - 1], 800);
                tcp.Start();
                List.Items.Add($"开始监听地址：{ipa[ipa.Length - 1].ToString()}");
            }
            else
                try
                {
                    tcp = new TcpListener(IPAddress.Parse(IPBox.Text), 800);
                    tcp.Start();
                    List.Items.Add($"开始监听地址：{IPBox.Text}");
                }
                catch
                {
                    MessageBox.Show("IP地址错误");
                }
            tcp.BeginAcceptSocket(In, tcp);
        }

        private void In(IAsyncResult ar)
        {
            Socket client = ar.AsyncState as Socket;
            if (sockets.Find((list) => { return list == client ? true : false; }) != null)
                sockets.Add(client);
            tcp.EndAcceptSocket(ar);
            client.BeginReceive(b, 0, b.Length, SocketFlags.None, InMessage, client);
            tcp.BeginAcceptSocket(In, tcp);
        }

        private void InMessage(IAsyncResult ar)
        {
            Socket client = ar.AsyncState as Socket;
            client.EndReceive(ar);
            Dispatcher.Invoke(() => { List.Items.Add(Encoding.Default.GetString(b).Replace("\0", "")); });
            client.BeginReceive(b, 0, b.Length, SocketFlags.None, InMessage, client);
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            foreach (var item in sockets)
            {
                byte[] b_ = Encoding.Default.GetBytes(Message.Text);
                item.BeginSend(b_, 0, b_.Length, SocketFlags.None, send, item);
            }
        }

        private void send(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;
            socket.EndSend(ar);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tcp.Stop();
        }
    }
}
