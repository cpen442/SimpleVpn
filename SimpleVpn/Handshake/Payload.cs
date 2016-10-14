using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn.Handshake
{
    class Payload
    {
        public Sender Sender { get; set; }
        public string ChallengeResponse { get; set; }
        public string PartialKey { get; set; }
    }
}
