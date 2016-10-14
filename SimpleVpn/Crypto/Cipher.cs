using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVpn.Crypto
{
    public class Cipher
    {
        private string _key { get; set; }

        public Cipher(string key)
        {
            _key = key;
        }

        public byte[] Encrypt(IEnumerable<byte> cipherText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + cipherText.ByteArrToStr(), ConsoleColor.Cyan);
            // TODO: make this work
            // format as E(input) in ascii
            var res = new List<byte>();
            res.Add(0x45); // E
            res.Add(0x28); // (
            res.AddRange(cipherText);
            res.Add(0x29); // )
            CConsole.WriteLine("Encrypted To: " + res.ByteArrToStr(), ConsoleColor.Yellow);
            return res.ToArray();
        }

        public byte[] Decrypt(IEnumerable<byte> cipherText)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            CConsole.WriteLine("Decrypting Bytes: " + cipherText.ByteArrToStr(), ConsoleColor.Yellow);

            // TODO: make this work
            // format as E(input) in ascii
            var res = new List<byte>();
            res.Add(0x44); // D
            res.Add(0x28); // (
            res.AddRange(cipherText);
            res.Add(0x29); // )
            CConsole.WriteLine("Decrypted To: " + res.ByteArrToStr(), ConsoleColor.Cyan);

            return res.ToArray();
        }
    }
}
