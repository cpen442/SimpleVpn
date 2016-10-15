using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;
using System.Numerics;

namespace SimpleVpn.Comms
{
    class Server
    {
        private Socket _listener;
        private Conversation _conversation;

        public Server(int port)
        {
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEndPoint);

            Console.WriteLine("Server running at: {0}:{1}", ipAddress, port);
        }

        public Conversation Converse(string sharedKey)
        {
            int backlog = 10;
            _listener.Listen(backlog);
            Console.WriteLine("Waiting for a client connection...");

            var handler = _listener.Accept();
            Console.WriteLine("Connected to client: {0}", handler.RemoteEndPoint.ToString());

            var newKey = ShakeHands(sharedKey);
            var cipher = new Cipher(newKey);

            _conversation = new Conversation(handler, cipher);

            var state = new SocketState();
            state.WorkSocket = handler;

            handler.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(_conversation.Listen), state);

            return _conversation;
        }

        private string ShakeHands(string sharedKey)
        {
            // ClntToSvr: "Client", Ra
            // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
            // ClntToSvr: E("Client", Rb, g^a modp, Kab)

            var newKey = "newkey";

            Console.WriteLine("Handshake complete, new key: {0}", newKey);

            return newKey;
        }
    }
}
