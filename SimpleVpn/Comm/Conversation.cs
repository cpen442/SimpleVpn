using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleVpn.Const;

namespace SimpleVpn.Comm
{
    public static class Conversation
    {
        public static void OnReceive(IAsyncResult ar)
        {

            SocketState state = (SocketState)ar.AsyncState;
            Socket client = state.workSocket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            }
            if (state.sb.ToString().Contains(Constants.EOF))
            {
                Console.WriteLine("Received: {0}", state.sb.ToString().Replace(Constants.EOF, ""));
                state.sb.Clear();
            }

            client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(OnReceive), state);
        }

        public static void Speak(string message, Socket socket)
        {
            var m = message + Constants.EOF;
            var msg = Encoding.ASCII.GetBytes(m);
            socket.Send(msg);
        }
    }
}
