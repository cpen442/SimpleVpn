using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comms
{
    public class Conversation
    {
        public bool IsLive;
        private Socket _socket;
        private Cipher _cipher;

        public Conversation(Socket socket, Cipher secret)
        {
            _socket = socket;
            _cipher = secret;
        }

        public void Listen(IAsyncResult ar)
        {            
            SocketState state = (SocketState)ar.AsyncState;
            Socket client = state.WorkSocket;

            try
            {
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.LongTermBuffer.AddRange(state.Buffer.Take(bytesRead));
                }
                if (state.LongTermBuffer.Last().Equals(Variables.EOF))
                {
                    var decrypted = _cipher.Decrypt(state.LongTermBuffer.Take(state.LongTermBuffer.Count - 1)); //remove the EOF byte then decrypt
                    var msg = Encoding.ASCII.GetString(decrypted);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine(Variables.ReceivedMsg + msg);
                    Console.Write(Variables.SendMsg);
                    state.LongTermBuffer.Clear();
                }

                client.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                    new AsyncCallback(Listen), state);
            }
            catch
            {
                Shutdown();

                Console.WriteLine("Conversation finished. Press any key to continue.");
            }
        }

        public void Speak(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            var bytes = Encoding.ASCII.GetBytes(message);
            var encrypted = _cipher.Encrypt(bytes);
            var sending = new List<byte>();
            sending.AddRange(encrypted);
            sending.Add(Variables.EOF);
            _socket.Send(sending.ToArray());
        }

        public void BeginReceive()
        {
            IsLive = true;
            var state = new SocketState { WorkSocket = _socket };
            _socket.BeginReceive(state.Buffer, 0, SocketState.BufferSize, 0,
                new AsyncCallback(Listen), state);
        }

        public void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            IsLive = false;
        }
    }
}