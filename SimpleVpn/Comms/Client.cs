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

        private Conversation ShakeHands(string sharedKey, Conversation convos)
        {
            //TODO: mutual authentication

            DiffieHellman DH = new DiffieHellman();

            // generate g^a mod p
            string a = "";  
            BigInteger DHa_val = 0;
            do
            {
                try
                {
                    Console.Write("Please enter your very own secret NUMBER:");
                    a = Console.ReadLine();
                    DHa_val = DH.hardComputeSharedDH(Convert.ToInt64(a));
                    CConsole.WriteLine("The g^a mod p value is:" + DHa_val, ConsoleColor.Red); // red for testing

                }
                catch (FormatException)
                {
                    Console.WriteLine("Error: enter a INTEGER number!");
                }
            } while (DHa_val == 0);

            // send this DHa value  
            convos.Speak(DHa_val.ToString());

            // listen for DHb value from server (TODO)
            BigInteger DHb_val = 21; // for testing 

            // a = 2
            // DHa_val = 7^2 % 23 = 3 
            //--> DH_final = 21^2 % 23 = 4
            

            // calculate DH value
            BigInteger DH_final = DH.hardComputeFinalDH(DHb_val, Convert.ToInt64(a));
            CConsole.WriteLine("The g^ab mod p value is:" + DH_final, ConsoleColor.Red); // red for testing

            // set final DH as session key
            convos.changeSecret(DH_final.ToString());

            return convos;
        }

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
