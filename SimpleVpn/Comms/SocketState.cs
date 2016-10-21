using System.Collections.Generic;
using System.Net.Sockets;

namespace SimpleVpn.Comms
{
    public class SocketState
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 256;
        public byte[] Buffer = new byte[BufferSize];
        public List<byte> LongTermBuffer = new List<byte>();
    }
}