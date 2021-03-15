using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RestClient.API
{

    public static class Api
    {
        const int BUFLEN = 10485760;
        private static byte[] buffer = new byte[BUFLEN];

        private static Socket sock;
        private static string LastIP = "";

        public static bool StartConnection( string IP, int port )
        {
            IPHostEntry host = Dns.GetHostEntry(IP);
            IPAddress ipAddress = host.AddressList[0];
            
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sock.Connect(remoteEP);

            return true;
        }

        public static string SendRequest(string remote, string header, string body)
        {
            try {
                if ((sock != null) && sock.Connected)
                    LastIP = "";
                if (LastIP != remote)
                {
                    LastIP = remote;

                    int port = 0;
                    if (remote.IndexOf("//") > -1)
                    {
                        string pro = remote.Substring(0, remote.IndexOf("//")+2);
                        remote = remote.Substring(remote.IndexOf("//") + 2);

                        if (remote.IndexOf(":") > -1)
                        {
                            port = int.Parse(remote.Substring(remote.IndexOf(":") + 1));
                            remote = remote.Substring(0, remote.IndexOf(":"));


                        }
                        if (port != 0)
                            StartConnection(remote, port);
                        else
                        {
                            switch (pro)
                            {
                                case "http://":
                                    StartConnection(remote, 80);
                                    break;
                                case "https://":
                                    StartConnection(remote, 453);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        port = int.Parse(remote.Substring(remote.IndexOf(":") + 1));
                        remote = remote.Substring(0, remote.IndexOf(":"));

                        StartConnection(remote, port);
                    }
                }

                header = header.Replace("\\r", "\r").Replace("\\n", "\n");

                sock.Send(Encoding.UTF8.GetBytes(header + body));

                byte[] msg = new byte[BUFLEN];
                int byteRecv = sock.Receive(msg);

                return Encoding.UTF8.GetString(msg,0,byteRecv);

            }
            catch {
                return "Error";
            }

            }
    }
}