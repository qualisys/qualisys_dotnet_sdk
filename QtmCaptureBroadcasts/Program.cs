using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace QtmCaptureBroadcasts
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int qtmBroadcastPort = 8989;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, qtmBroadcastPort);
                socket.Bind(endPoint);

                Console.WriteLine($"Listening for Qualisys Track Manager UDP broadcasts on port: {qtmBroadcastPort}");

                byte[] buffer = new byte[1601];
                while (true)
                {
                    var read = socket.Receive(buffer);
                    if (read > 0)
                    {
                        var xml = Encoding.UTF8.GetString(buffer).Trim('\0');
                        try
                        {
                            XDocument doc = XDocument.Parse(xml);
                            Console.WriteLine(doc.ToString());
                        }
                        catch (Exception)
                        {
                            Console.WriteLine(xml);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
