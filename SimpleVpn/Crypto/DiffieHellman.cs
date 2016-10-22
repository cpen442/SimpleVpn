using System;
using System.Numerics;
using SimpleVpn.Constants;

namespace SimpleVpn.Crypto
{
    class DiffieHellman
    {
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
            var p = BigInteger.Parse(Variables.P);
            var g = BigInteger.Parse(Variables.G);

            return BigInteger.ModPow(g, n, p);
        }

        /* calculates a DH value using hardcoded P, chosen secret m, and shared DH value: [(G^n)^m mod P] */
        public BigInteger hardComputeFinalDH(BigInteger sharedDH, BigInteger m)
        {
            var p = BigInteger.Parse(Variables.P);

            return BigInteger.ModPow(sharedDH, m, p);
        }
    }
}