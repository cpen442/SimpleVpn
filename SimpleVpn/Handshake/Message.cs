using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn.Handshake
{
    class Message
    {
        public Sender Sender { get; set; }
        public string Challenge { get; set; }
        public string EncryptedPayload { get; set; }
    }
}
