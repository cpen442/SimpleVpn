using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleVpn.Comms;
using SimpleVpn.Constants;
using System.Numerics;

namespace SimpleVpn
{
    class Program
    {
        
        static void Main(string[] args)
        {

            Console.Write("Please enter {0} for Server mode, or {1} for client mode: ", (int)Mode.Server, (int)Mode.Client);
            var selectedMode = Console.ReadLine();
            var mode = Convert.ToInt32(selectedMode);

            Conversation conversation;

            switch (mode)
            {
                case (int)Mode.Server:

                    Console.Write("Please enter the server port: ");
                    var inputPort = Console.ReadLine();
                    var port = Convert.ToInt32(inputPort);

                    var server = new Server(port);

                    Console.Write("Please enter a secret shared key:");
                    string sharedKey = Console.ReadLine();
                    conversation = server.Converse(sharedKey);

                    break;

                case (int)Mode.Client:

                    Console.Write("Server IP address: ");
                    var inputSvrIpAddr = Console.ReadLine();
                    var svrIpAddr = IPAddress.Parse(inputSvrIpAddr);

                    Console.Write("Server port: ");
                    var inputSvrPort = Console.ReadLine();
                    var svrPort = Convert.ToInt32(inputSvrPort);

                    var client = new Client(svrIpAddr, svrPort);
                    Console.Write("Please enter a shared secret key:");
                    string sharedKey_ = Console.ReadLine();
                    conversation = client.Converse(sharedKey_);

                    break;

                default:
                    throw new ArgumentException("Please enter a valid mode of opeartion.");
            }

            while (true)
            {
                Console.Write(Variables.SendMsg);
                var msg = Console.ReadLine();

                conversation.Speak(msg);
            }
            
        }
    }
}
