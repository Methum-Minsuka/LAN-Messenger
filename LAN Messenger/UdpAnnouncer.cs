using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpAnnouncer
{
    private UdpClient udpClient;
    private int port = 8888;

    public void Announce(string message)
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;

        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
        udpClient.Close();
    }
}
