using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SimpleVpn.Handshake
{
    class Message
    {
        // feel free to change strings to byte[] or BigInteger if needed.

        public Sender Sender { get; set; }
        public string Challenge { get; set; }
        public string EncryptedPayload { get; set; }
    }
}
