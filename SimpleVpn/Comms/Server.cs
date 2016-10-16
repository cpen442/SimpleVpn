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

        // creates a server
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

            // initiate conversation
            _conversation = new Conversation(handler, new Cipher(sharedKey)); 

            // create socket
            var state = new SocketState();
            state.WorkSocket = handler;

            // connect
            handler.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(_conversation.Listen), state);

            // pass the conversation to handshake
            _conversation = ShakeHands(sharedKey, _conversation);

            return _conversation;
        }

        // returns the conversation with DH value as session key
        /* PROTOCOL IS AS FOLLOWS: */
        // ClntToSvr: "Client", Ra
        // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
        // ClntToSvr: E("Client", Rb, g^a modp, Kab)
        private Conversation ShakeHands(string sharedKey, Conversation _conversation)
        {

            /* var generator = 5;
             var prime = 23;
             var serverSecret = 7; //can be a string

             var DHval = ShakeHands(sharedKey); // get DH value
             var cipher = new Cipher(DHval.ToString()); // make DH the session key

             _conversation = new Conversation(handler, cipher); // start a conversation

             int serverDH = (int)Math.Pow(generator, serverSecret) % prime;
             Console.WriteLine("serverDH: {0}", serverDH);

             int DHvalue = serverDH;
             Console.WriteLine("Handshake complete, final server-computed DH is: {0}", DHvalue);*/

            return _conversation;
        }
    }
}
