using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleVpn
{
    class Server
    {
        Socket _listener;

        public Server(int port)
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEndPoint);

            Console.WriteLine("Server running at: {0}:{1}", ipAddress, port);
        }

        public void Listen()
        {
            int backlog = 10;
            _listener.Listen(backlog);

            Console.WriteLine("Waiting for a client connection...");
            Socket handler = _listener.Accept();
            Console.WriteLine("Connected to client: {0}", handler.RemoteEndPoint.ToString());

            while (true)
            {              
                var data = String.Empty;

                while (true)
                {
                    var buffer = new byte[1024];
                    var bytesRec = handler.Receive(buffer);
                    data += Encoding.ASCII.GetString(buffer, 0, bytesRec);
                    if (data.IndexOf(Constants.EOF) > -1)
                    {
                        break;
                    }
                }

                Console.WriteLine("Received: {0}", data);
            }
        }
    }
}
