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
        public Conversation Converse(string sharedKey)
        {
            _sender.Connect(_remoteEndpoint);
            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());
            
            // initiate conversation
            _conversation = new Conversation(_sender, new Cipher(sharedKey));
            
            // connect
            var state = new SocketState { WorkSocket = _sender };
            _sender.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(_conversation.Listen), state);

            // pass the conversation to handshake
            _conversation = ShakeHands(sharedKey, _conversation);

            return _conversation;
        }


        // PROTOCOL:
        // ClntToSvr: "Client", Ra
        // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
        // ClntToSvr: E("Client", Rb, g^a modp, Kab)

        private Conversation ShakeHands(string sharedKey, Conversation _conversation)
        {
            /*var generator;
            var prime;
            var clientSecret = 2; // can be string
           
            int clientDH = (int)Math.Pow(generator, Convert.ToInt64(clientSecret)) % prime;
            Console.WriteLine("client DH value: {0}", clientDH);
            */
            return _conversation;
        }

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
