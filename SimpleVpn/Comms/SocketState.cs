using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
