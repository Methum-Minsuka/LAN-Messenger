using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LAN_Messenger
{
    public class UdpListener : IDisposable
    {
        private readonly int port;
        private UdpClient udp;
        private Thread listenThread;
        private readonly HashSet<string> onlineByIp = new HashSet<string>();
        private string myName;

        public delegate void UserDetectedHandler(string userInfo);
        public event UserDetectedHandler OnUserDetected;

        public UdpListener(int port = 8888)
        {
            this.port = port;
        }

        public void StartListening(string myName)
        {
            this.myName = myName ?? "Unknown";

            // create and bind socket (so replies will be sent/received on same port)
            udp = new UdpClient();

            // allow reuse so multiple apps on same machine can bind the same port
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.EnableBroadcast = true;
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            listenThread = new Thread(ListenLoop) { IsBackground = true };
            listenThread.Start();
        }

        private void ListenLoop()
        {
            try
            {
                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udp.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    if (string.IsNullOrEmpty(message))
                        continue;

                    // Request from a new app asking who is online
                    if (message.StartsWith("WHO_IS_ONLINE", StringComparison.OrdinalIgnoreCase))
                    {
                        // Reply directly to the requester (using same bound udp so source port is our listening port)
                        byte[] reply = Encoding.UTF8.GetBytes($"ONLINE|{myName}");
                        udp.Send(reply, reply.Length, remoteEP);
                    }
                    else if (message.StartsWith("ONLINE|", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = message.Split(new[] { '|' }, 2);
                        if (parts.Length >= 2)
                        {
                            string name = parts[1].Trim();
                            string ip = remoteEP.Address.ToString();

                            // ignore our own announcement (same machine)
                            if (IsLocalAddress(remoteEP.Address))
                                continue;

                            lock (onlineByIp)
                            {
                                if (!onlineByIp.Contains(ip))
                                {
                                    onlineByIp.Add(ip);
                                    OnUserDetected?.Invoke($"{name} ({ip})");
                                }
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // socket closed during Dispose - normal shutdown
            }
            catch (SocketException)
            {
                // ignore or log if needed
            }
            catch (Exception)
            {
                // ignore for this demo
            }
        }

        // Broadcast that we are online (uses the bound udp so replies will be directed correctly)
        public void AnnounceOnline()
        {
            if (udp == null) return;
            byte[] data = Encoding.UTF8.GetBytes($"ONLINE|{myName}");
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
        }

        // Broadcast WHO_IS_ONLINE (asks all existing apps to reply to us)
        public void RequestOnline()
        {
            if (udp == null) return;
            byte[] data = Encoding.UTF8.GetBytes("WHO_IS_ONLINE");
            udp.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
        }

        private bool IsLocalAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address)) return true;
            try
            {
                var hostAddrs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var a in hostAddrs)
                {
                    if (a.Equals(address)) return true;
                }
            }
            catch { }
            return false;
        }

        public void Dispose()
        {
            try
            {
                udp?.Close();
                udp?.Dispose();
            }
            catch { }

            try
            {
                listenThread?.Abort();
            }
            catch { }
        }
    }
}
