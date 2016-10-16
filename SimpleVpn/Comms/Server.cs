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

        /* PROTOCOL IS AS FOLLOWS: */
        // ClntToSvr: "Client", Ra
        // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
        // ClntToSvr: E("Client", Rb, g^a modp, Kab)
        private Conversation ShakeHands(string sharedKey, Conversation convos)
        {
            //TODO: mutual authentication

            DiffieHellman DH = new DiffieHellman();

            // generate g^b mod p
            string b = "";
            BigInteger DHb_val = 0;
            do
            {
                try
                {
                    Console.Write("Please enter your very own secret number:");
                    b = Console.ReadLine();
                    DHb_val = DH.hardComputeSharedDH(Convert.ToInt64(b));
                    CConsole.WriteLine("The g^b mod p value is:" + DHb_val, ConsoleColor.Red); //red for testing
                }
                catch (FormatException)
                {
                    Console.WriteLine("Error: enter a INTEGER number!");
                }

            } while (DHb_val == 0);

            // send this DHb value 
            convos.Speak(DHb_val.ToString());
            
            // listen for DHa value from client (TODO)
            String msg = "";
            BigInteger DHa_val = 0;
            do
            {
                msg = convos._message;
                DHa_val = 2;//BigInteger.Parse(msg);
            } while (msg.Equals(""));

            CConsole.WriteLine("The g^a mod p value RECEIVED is:" + msg, ConsoleColor.Red); // red for testing
            convos._secretSet = true;

            // calculate DH value
            BigInteger DH_final = DH.hardComputeFinalDH(DHa_val, Convert.ToInt64(b));
            CConsole.WriteLine("The g^ab mod p value is:" + DH_final, ConsoleColor.Red); // red for testing

            // set final DH as session key for encrypt/decrypt
            convos.changeSecret(DH_final.ToString());
            convos._DHset = true;

            return convos;
        }
    }
}
