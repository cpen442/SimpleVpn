using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Const;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comm
{
    class Server
    {
        private Socket _listener;
        private Conversation conv;

        public Server(int port)
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEndPoint);

            Console.WriteLine("Server running at: {0}:{1}", ipAddress, port);
        }

        public Conversation Listen()
        {
            int backlog = 10;
            _listener.Listen(backlog);

            Console.WriteLine("Waiting for a client connection...");
            var handler = _listener.Accept();
            Console.WriteLine("Connected to client: {0}", handler.RemoteEndPoint.ToString());

            SocketState state = new SocketState();
            state.workSocket = handler;
            
            //TODO:handshake here
            var secret = new Secret();
            //

            this.conv = new Conversation(handler, secret);

            handler.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(this.conv.OnReceive), state);

            return conv;
        }
    }
}
