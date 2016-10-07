using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Const;

namespace SimpleVpn.Comm
{
    class Server
    {
        private Socket _listener;

        public Server(int port)
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEndPoint);

            Console.WriteLine("Server running at: {0}:{1}", ipAddress, port);
        }

        public Socket Listen()
        {
            int backlog = 10;
            _listener.Listen(backlog);

            Console.WriteLine("Waiting for a client connection...");
            var handler = _listener.Accept();
            Console.WriteLine("Connected to client: {0}", handler.RemoteEndPoint.ToString());

            SocketState state = new SocketState();
            state.workSocket = handler;

            handler.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(Conversation.OnReceive), state);

            return handler;
        }
    }
}
