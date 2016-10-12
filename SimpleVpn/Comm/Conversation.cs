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
                state.longTermBuffer.AddRange(state.buffer.Take(bytesRead));
            }
            if (state.longTermBuffer.Last().Equals(Constants.EOF))
            {
                var decrypted = secret.Decrypt(state.longTermBuffer.Take(state.longTermBuffer.Count-1)); //remove the EOF byte then decrypt
                var msg = Encoding.ASCII.GetString(decrypted);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine(Constants.ReceivedMsg + msg);
                Console.Write(Constants.SendMsg);
                state.longTermBuffer.Clear();
            }

            client.BeginReceive(state.buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(OnReceive), state);
        }

        public void Speak(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            var encrypted = this.secret.Encrypt(bytes);
            //appending EOF byte
            var sending = new List<byte>();
            sending.AddRange(encrypted);
            sending.Add(Constants.EOF);
            this.socket.Send(sending.ToArray());
        }
    }
}
