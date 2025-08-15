using System.Net.Sockets;
using System.Net;
using System.Text;

public class UdpAnnouncer
{
    private int port = 8888;

    public void AnnounceOnline(string userName)
    {
        Broadcast($"ONLINE|{userName}");
    }

    public void RequestOnline()
    {
        Broadcast("WHO_IS_ONLINE");
    }

    private void Broadcast(string message)
    {
        using (UdpClient udp = new UdpClient())
        {
            udp.EnableBroadcast = true;
            byte[] data = Encoding.UTF8.GetBytes(message);
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 8888));
        }
    }

}
