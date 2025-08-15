using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpAnnouncer
{
    private int port = 8888;

    public void Announce(string message)
    {
        using (UdpClient udp = new UdpClient())
        {
            udp.EnableBroadcast = true;
            byte[] data = Encoding.UTF8.GetBytes(message);
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
        }
    }
}
