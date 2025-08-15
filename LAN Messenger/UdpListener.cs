using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class UdpListener
{
    private UdpClient udpClient;
    private int port = 8888;

    public delegate void UserDetectedHandler(string userInfo);
    public event UserDetectedHandler OnUserDetected;

    public void StartListening()
    {
        udpClient = new UdpClient(port);
        udpClient.EnableBroadcast = true;

        new Thread(() =>
        {
            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);

                OnUserDetected?.Invoke($"{message} ({remoteEP.Address})");
            }
        }).Start();
    }
}
