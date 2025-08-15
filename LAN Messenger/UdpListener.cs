using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UdpListener
{
    private int port = 8888;
    public delegate void UserDetectedHandler(string userInfo);
    public event UserDetectedHandler OnUserDetected;

    public void StartListening(string myName)
    {
        new Thread(() =>
        {
            using (UdpClient udp = new UdpClient())
            {
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.ExclusiveAddressUse = false;
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, 8888));

                HashSet<string> onlineUsers = new HashSet<string>();

                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 8888);
                    byte[] data = udp.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    if (message.StartsWith("WHO_IS_ONLINE"))
                    {
                        // Respond only to requester
                        UdpClient responder = new UdpClient();
                        byte[] reply = Encoding.UTF8.GetBytes($"ONLINE|{myName}");
                        responder.Send(reply, reply.Length, remoteEP);
                        responder.Close();
                    }
                    else if (message.StartsWith("ONLINE|"))
                    {
                        string user = message.Split('|')[1] + $" ({remoteEP.Address})";
                        if (onlineUsers.Add(user))
                            OnUserDetected?.Invoke(user);
                    }
                }
            }
        })
        { IsBackground = true }.Start();
    }


}
