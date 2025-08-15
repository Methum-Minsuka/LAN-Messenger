using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UdpListener
{
    private int port = 8888;
    public delegate void UserDetectedHandler(string userInfo);
    public event UserDetectedHandler OnUserDetected;

    public void StartListening()
    {
        new Thread(() =>
        {
            using (UdpClient udp = new UdpClient())
            {
                // Allow multiple apps to bind the same port
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.ExclusiveAddressUse = false;

                udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                    byte[] data = udp.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    OnUserDetected?.Invoke($"{message} ({remoteEP.Address})");
                }
            }
        })
        { IsBackground = true }.Start();
    }

}
