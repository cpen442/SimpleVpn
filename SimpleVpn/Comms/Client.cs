using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;
using SimpleVpn.Handshake;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Numerics;

namespace SimpleVpn.Comms
{
    class Client
    {
        private Socket _sender;
        private IPEndPoint _remoteEndpoint;
        private Conversation _conversation;

        // creates a client
        public Client(IPAddress svrIpAddr, int svrPort)
        {
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _remoteEndpoint = new IPEndPoint(svrIpAddr, svrPort);
        }

        // communicate between client and server using secret key
        public Conversation Converse(string passwd)
        {
            _sender.Connect(_remoteEndpoint);
            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());

            // shake hands
            var hs = new Handshake(_sender,passwd);
            var sessionKey = hs.AsClient();

            // initiate conversation
            _conversation = new Conversation(_sender, new Cipher(sessionKey));

            // connect
            var state = new SocketState { WorkSocket = _sender };
            _sender.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(_conversation.Listen), state);
            
            return _conversation;
        }
        
        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
