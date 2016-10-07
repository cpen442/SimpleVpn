using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn.Crypto
{
    public class Secret
    {
        public void SetSessionKey(string key)
        {
            throw new NotImplementedException();
        }

        public string Encrypt(string input)
        {
            //throw new NotImplementedException();
            return "E(" + input + ")";
        }

        public string Decrypt(string input)
        {
            //throw new NotImplementedException();
            return "D(" + input + ")";
        }
    }
}
