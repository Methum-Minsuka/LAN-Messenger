using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpAnnouncer
{
    private int port = 8888;

    public void AnnounceOnline(string userName)
    {
        string message = $"ONLINE|{userName}";
        Broadcast(message);
    }

    public void RequestOnline()
    {
        string message = "WHO_IS_ONLINE";
        Broadcast(message);
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
