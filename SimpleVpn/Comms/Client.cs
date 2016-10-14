using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;
using SimpleVpn.Handshake;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SimpleVpn.Comms
{
    class Client
    {
        private Socket _sender;
        private IPEndPoint _remoteEndpoint;
        private Conversation _conversation;

        public Client(IPAddress svrIpAddr, int svrPort)
        {
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _remoteEndpoint = new IPEndPoint(svrIpAddr, svrPort);
        }

        public Conversation Converse(string sharedKey)
        {
            _sender.Connect(_remoteEndpoint);
            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());

            var newKey = ShakeHands(sharedKey);
            var cipher = new Cipher(newKey);

            _conversation = new Conversation(_sender, cipher);

            var state = new SocketState { WorkSocket = _sender };
            _sender.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
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

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
