using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleVpn.Const;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comm
{
    public class Conversation
    {
        private Socket socket;
        private Secret secret;

        public Conversation(Socket socket, Secret secret)
        {
            this.socket = socket;
            this.secret = secret;
        }

        public void OnReceive(IAsyncResult ar)
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
                Console.SetCursorPosition(0,Console.CursorTop);
                var msg = state.sb.ToString().Replace(Constants.EOF, "");
                Console.WriteLine(Constants.ReceivedMsg +  this.secret.Decrypt(msg));
                Console.Write(Constants.SendMsg);
                state.sb.Clear();
            }

            client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(OnReceive), state);
        }

        public void Speak(string message)
        {
            var m = this.secret.Encrypt(message) + Constants.EOF;
            var msg = Encoding.ASCII.GetBytes(m);
            this.socket.Send(msg);
        }
    }
}
