using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn
{
    class Client
    {
        private Socket _sender;

        public Client(IPAddress svrIpAddr, int svrPort)
        {
            var remoteEP = new IPEndPoint(svrIpAddr, svrPort);
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _sender.Connect(remoteEP);

            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint.ToString());
        }

        public void Speak(string message)
        {
            var m = message + Constants.EOF;
            var msg = Encoding.ASCII.GetBytes(m);
            _sender.Send(msg);
        }

        public void Shutdown()
        {
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }
    }
}
