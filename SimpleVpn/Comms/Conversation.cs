using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comms
{
    public class Conversation
    {
        private Socket _socket;
        private Cipher _cipher;
        public String _message;
        public bool _DHset;
        public bool _secretSet;

        // creates a conversation
        public Conversation(Socket socket, Cipher secret)
        {
            _socket = socket;
            _cipher = secret;
            _message = "";
            _DHset = false;
            _secretSet = false;
        }

        // changes the current key to the given key (Kab to DHkey)
        public void changeSecret(String secret) {
            _cipher._key = secret;
        }
        
        public void Listen(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;
            Socket client = state.WorkSocket;

            int bytesRead = client.EndReceive(ar); //end read

            if (bytesRead > 0)
            {
                state.LongTermBuffer.AddRange(state.Buffer.Take(bytesRead));
            }
            if (state.LongTermBuffer.Last().Equals(Variables.EOF))
            {
                var decrypted = _cipher.Decrypt(state.LongTermBuffer.Take(state.LongTermBuffer.Count-1)); //remove the EOF byte then decrypt
                var msg = Encoding.ASCII.GetString(decrypted); 
                _message = msg;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine(Variables.ReceivedMsg + msg);

                // message prefixes
                if (!_secretSet)
                    Console.Write("Please enter your very own secret number:");
                else if (_DHset)
                    Console.Write(Variables.SendMsg);

                state.LongTermBuffer.Clear();
            }

            // new read
            client.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(Listen), state);

        }

        public void Speak(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            var encrypted = bytes;
    
            encrypted = _cipher.Encrypt(bytes);
          
            var sending = new List<byte>();
            sending.AddRange(encrypted);
            sending.Add(Variables.EOF);
            _socket.Send(sending.ToArray());
        }
    }
}
