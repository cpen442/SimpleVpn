using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SimpleVpn.Crypto
{
    class DiffieHellman
    {
        /*hard coded public constants*/
        // P is a 1080 bit prime 
        public static String P = "121999492637070040497464880653482451122159715698431661862504934268987469885677710797799523307422120568454593141727668682332216679465216347609718241998150443969871262326615939878834844507147192404574401325870276945218845272041195113380201145626974399759092850500988371156171063899568397919181947787377580179491"; //23
        // G is a 160 bit prime 
        public static String G = "5322540848240629191790738444031509035347191367506640093778351221586257526250970342938004178185797187285021641998886132756315601193005759267241157022234467562219"; //7

        /* calculates a DH value using [g^n mod p] */
        public BigInteger computeDH(BigInteger g, BigInteger n, BigInteger p)
        {
            return BigInteger.ModPow(g, n, p);
        }

        public BigInteger computeDH(int g, int n, int p)
        {
            return BigInteger.ModPow(g, n, p);
        }

        /* calculates a DH value using hardcoded G,P: [G^n mod P] */
        public BigInteger hardComputeSharedDH(BigInteger n)
        {
            var p = BigInteger.Parse(P);
            var g = BigInteger.Parse(G);

            return BigInteger.ModPow(g, n, p);
        }

        /* calculates a DH value using hardcoded P, chosen secret m, and shared DH value: [(G^n)^m mod P] */
        public BigInteger hardComputeFinalDH(BigInteger sharedDH, BigInteger m)
        {
            var p = BigInteger.Parse(P);

            return BigInteger.ModPow(sharedDH, m, p);
        }
    }
}