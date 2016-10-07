using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Const;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comm
{
    class Client
    {
        private Socket _sender;
        private IPEndPoint remoteEP;
        private Conversation conv;

        public Client(IPAddress svrIpAddr, int svrPort)
        {
            remoteEP = new IPEndPoint(svrIpAddr, svrPort);
        }

        public Conversation Connect()
        {
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _sender.Connect(remoteEP);

            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());


            SocketState state = new SocketState();
            state.workSocket = _sender;

            //TODO:handshake here
            var secret = new Secret();
            //

            this.conv = new Conversation(_sender, secret);

            _sender.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(this.conv.OnReceive), state);
            return conv;
        }

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
