using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using SimpleVpn.Constants;
using SimpleVpn.Crypto;

namespace SimpleVpn.Comms
{
    public class Handshake
    {
        private Socket _socket;
        private byte[] key;
        private Cipher cipher;

        public Handshake(Socket socket, string passwd)
        {
            this._socket = socket;
            this.key = getHashSha256(passwd);
            cipher = new Cipher(key);
            CConsole.WriteLine("Handshake Starting", ConsoleColor.Green, nostep: true);
            CConsole.WriteLine("Using Handshake Key (SHA256) of " + passwd + " : " + this.key.ByteArrToStr(), ConsoleColor.Green);
        }


        /* PROTOCOL IS AS FOLLOWS: */
        // ClntToSvr: "Client", Ra
        // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
        // ClntToSvr: E("Client", Rb, g^a modp, Kab)
        public byte[] AsServer()
        {
            //wait for: ClntToSvr: "Client", Ra
            CConsole.Write("Waiting for Message: 'Client',Ra :", ConsoleColor.Green);
            var rcvd = WaitMessageSync();
            Console.WriteLine("");

            if (rcvd.First() != (byte)ModeByte.Client)
            {
                throw new UnauthorizedAccessException("received message is not sent from client");
            }
            var RaBytes = rcvd.Skip(1);
            CConsole.WriteLine("I am challenged with Ra: " + RaBytes.ByteArrToStr(), ConsoleColor.Green);
            Console.WriteLine("");

            var r = new Random();
            var RbBytes = RandomBytes(Variables.RaRbLength, r);

            CConsole.WriteLine("Challenging with Rb: " + RbBytes.ByteArrToStr(), ConsoleColor.Green);
            Console.WriteLine("");

            DiffieHellman DH = new DiffieHellman();

            // generate g^b mod p
            var b = BigInteger.Abs(new BigInteger(RandomBytes(Variables.DHCoefficientLength, r)));
            var DHb_val = DH.hardComputeSharedDH(b);
            CConsole.WriteLine("My D-H coefficient chosen: " + b, ConsoleColor.Green);
            Console.WriteLine("");
            CConsole.WriteLine("Sending D-H g^b mod p value: " + DHb_val, ConsoleColor.Green);
            Console.WriteLine("");

            var enc = new List<byte>(); //corresponds to E("Svr", Ra, g^b modp, Kab)

            enc.Add((byte)ModeByte.Server);
            enc.AddRange(RaBytes);
            enc.AddRange(DHb_val.ToByteArray());
            enc = cipher.Encrypt(enc).ToList<byte>();

            var m = new List<byte>();
            m.AddRange(RbBytes);
            m.AddRange(enc);
            // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
            CConsole.WriteLine("Sending Rb, E('Sever',Ra, g^b mod p) " + DHb_val, ConsoleColor.Green);
            SendMessageSync(m);
            Console.WriteLine("");

            // wait for: ClntToSvr: E("Client", Rb, g^a modp, Kab)
            CConsole.Write("Waiting for Message: E('Client', Rb, g^a mod p ):", ConsoleColor.Green);
            rcvd = cipher.Decrypt(WaitMessageSync());

            if (rcvd.First() != (byte)ModeByte.Client)
            {
                throw new UnauthorizedAccessException("received message did not come from client, Or the passwords do not match.");
            }
            //if Rb != received and decrypted rb
            if (!ByteArrMatches(rcvd.Skip(1).Take(Variables.RaRbLength), (RbBytes)))
            {
                throw new UnauthorizedAccessException("Password mismatch");
            }
            var gamodp = rcvd.Skip(1 + Variables.RaRbLength).ToArray();
            var DHa_val = new BigInteger(gamodp);

            CConsole.WriteLine("Received D-H g^b mod p value: " + DHa_val, ConsoleColor.Green);
            Console.WriteLine("");

            // calculate DH value
            BigInteger DH_final = DH.hardComputeFinalDH(DHa_val, b);
            CConsole.WriteLine("The g^ab mod p value is:" + DH_final, ConsoleColor.Green);
            Console.WriteLine("");
            CConsole.WriteLine("Key is: " + DH_final.ToByteArray().ByteArrToStr(),ConsoleColor.Magenta);

            return DH_final.ToByteArray();
        }


        // PROTOCOL:
        // ClntToSvr: "Client", Ra
        // SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
        // ClntToSvr: E("Client", Rb, g^a modp, Kab)
        public byte[] AsClient()
        {
            var r = new Random();
            // ClntToSvr: "Client", Ra
            var RaBytes = RandomBytes(Variables.RaRbLength, r);

            CConsole.WriteLine("Challenging with Ra: " + RaBytes.ByteArrToStr(), ConsoleColor.Green);
            Console.WriteLine("");

            var m = new List<byte>();
            m.Add((byte)ModeByte.Client);
            m.AddRange(RaBytes);

            CConsole.Write("Sending Message: 'Client',Ra:", ConsoleColor.Green);
            SendMessageSync(m);
            Console.WriteLine("");

            // wait for: SvrToClnt: Rb, E("Svr", Ra, g^b modp, Kab)
            CConsole.Write("Waiting for Message: Rb, E('Server', Ra, g^b mod p ):", ConsoleColor.Green);
            var rcvd = WaitMessageSync().ToList();
            Console.WriteLine("");

            //given above uint, expect first {Variables.RaRbLength} bytes as RB, rest as E("Svr",Ra, g^b modp, Kab)
            var Rb = rcvd.Take(Variables.RaRbLength).ToArray();
            CConsole.WriteLine("I am challenged with Rb: " + Rb.ByteArrToStr(), ConsoleColor.Green);
            Console.WriteLine("");

            var dec = cipher.Decrypt(rcvd.Skip(Variables.RaRbLength));

            if (dec.First() != (byte)ModeByte.Server)
            {
                throw new UnauthorizedAccessException("Received message did not come from server, Or the password do not match.");
            }

            //if Ra != received and decrypted ra
            if (!ByteArrMatches(dec.Skip(1).Take(Variables.RaRbLength), RaBytes))
            {
                throw new UnauthorizedAccessException("Password mismatch");
            }
            CConsole.WriteLine("Received Ra challenge response and passed: " + RaBytes.ByteArrToStr(), ConsoleColor.Green);
            Console.WriteLine("");

            var gbmodp = dec.Skip(1 + Variables.RaRbLength).ToArray();
            BigInteger DHb_val = new BigInteger(gbmodp);
            CConsole.WriteLine("Received D-H g^b mod p value: " + DHb_val, ConsoleColor.Green);
            Console.WriteLine("");

            DiffieHellman DH = new DiffieHellman();
            // generate g^a mod p
            var a = BigInteger.Abs(new BigInteger(RandomBytes(Variables.DHCoefficientLength, r)));
            var DHa_val = DH.hardComputeSharedDH(a);
            CConsole.WriteLine("My D-H coffefficient chosen: " + a, ConsoleColor.Green);
            Console.WriteLine("");

            CConsole.WriteLine("Sending my D-H g^a mod p value : " + DHa_val, ConsoleColor.Green);
            Console.WriteLine("");

            // ClntToSvr: E("Client", Rb, g^a modp, Kab)
            m.Clear();
            m.Add((byte)ModeByte.Client);
            m.AddRange(Rb);
            m.AddRange(DHa_val.ToByteArray());
            m = cipher.Encrypt(m).ToList<byte>();


            CConsole.Write("Sending Message: E('Client', Rb, g^a mod p) :", ConsoleColor.Green);
            SendMessageSync(m);
            Console.WriteLine("");


            // calculate DH value
            BigInteger DH_final = DH.hardComputeFinalDH(DHb_val, a);
            CConsole.WriteLine("The g^ab mod p value is:" + DH_final, ConsoleColor.Green);
            Console.WriteLine("");
            CConsole.WriteLine("Key is: " + DH_final.ToByteArray().ByteArrToStr(), ConsoleColor.Magenta);

            return DH_final.ToByteArray();
        }

        private void SendMessageSync(IEnumerable<byte> m)
        {
            CConsole.WriteLine("SENT: " + m.ByteArrToStr(), ConsoleColor.DarkGreen);
            _socket.Send(m.ToArray());
        }

        private IEnumerable<byte> WaitMessageSync()
        {
            var buffer = new List<byte>();
            while (true)
            {
                var _buffer = new byte[5000]; // made this large enough. should we make it better?
                var bytesRec = _socket.Receive(_buffer);
                buffer.AddRange(_buffer.Take(bytesRec));
                if (bytesRec < 5000) break;
            };
            CConsole.WriteLine("RCVD: " + buffer.ByteArrToStr(), ConsoleColor.DarkGreen);
            return buffer;
        }

        private byte[] getHashSha256(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            return hash;
        }

        private bool ByteArrMatches(IEnumerable<byte> one, IEnumerable<byte> two)
        {
            if (one.Count() != two.Count())
            {
                return false;
            }
            for (var i = 0; i < one.Count(); i++)
            {
                if (!one.ElementAt(i).Equals(two.ElementAt(i)))
                {
                    return false;
                }
            }
            return true;
        }

        private byte[] RandomBytes(int length, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random();
            }
            var buff = new byte[length];
            rand.NextBytes(buff);

            return buff;
        }
    }
}