using NetSocket;
using NetSocket.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UDPSender
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private UdpSocket UdpSocket { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            UdpSocket = new UdpSocket();
            UdpSocket.Start(10086);
        }

        private void TxtProtocolID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (UdpSocket != null)
            {
                CommPacket oCommPacket = new CommPacket()
                {
                    PacketID = int.Parse(TxtProtocolID.Text),
                    Content = TxtProtoclContet.Text,
                };
                UdpSocket.UserToken.SendAsync(oCommPacket);
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDestory_Click(object sender, RoutedEventArgs e)
        {
            if (UdpSocket != null)
            {
                UdpSocket.CloseSocket();
            }
        }
    }
}

public class CommPacket : IPacket
{
    public object Owner { get; set; }
    public int PacketID { get; set; }
    public string Content { get; set; }

    public void Deserialization(DynamicBuffer oBuffer)
    {
        Content = oBuffer.ReadUTF8();
    }

    public void Serialization(DynamicBuffer oBuffer)
    {
        oBuffer.WriteUTF8(Content);
    }
}
