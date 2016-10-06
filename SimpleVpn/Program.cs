using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter {0} for Server mode, or {1} for client mode: ", Mode.Server, Mode.Client);
            var selectedMode = Console.ReadLine();
            var mode = Convert.ToInt32(selectedMode);

            if (mode == (int)Mode.Server)
            {
                RunServer();
            }
            else if(mode == (int)Mode.Client)
            {
                RunClient();
            } else
            {
                Console.WriteLine("Please enter a valid mode of opeartion.");
            }
        }

        static void RunServer()
        {
            Console.Write("Please enter the server port: ");
            var inputPort = Console.ReadLine();
            var port = Convert.ToInt32(inputPort);

            var server = new Server(port);
            server.Listen();
        }

        static void RunClient()
        {
            Console.Write("Server IP address: ");
            var inputSvrIpAddr = Console.ReadLine();
            var svrIpAddr = IPAddress.Parse(inputSvrIpAddr);

            Console.Write("Server port: ");
            var inputSvrPort = Console.ReadLine();
            var svrPort = Convert.ToInt32(inputSvrPort);

            var client = new Client(svrIpAddr, svrPort);

            while (true)
            {
                Console.Write("To send: ");
                var msg = Console.ReadLine();

                client.Speak(msg);
            }
        }
    }
}
