using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Const;

namespace SimpleVpn.Comm
{
    class Client
    {
        private Socket _sender;
        private IPEndPoint remoteEP;

        public Client(IPAddress svrIpAddr, int svrPort)
        {
            remoteEP = new IPEndPoint(svrIpAddr, svrPort);
        }

        public Socket Connect()
        {
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _sender.Connect(remoteEP);

            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());


            SocketState state = new SocketState();
            state.workSocket = _sender;

            _sender.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(Conversation.OnReceive), state);
            return _sender;
        }

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
