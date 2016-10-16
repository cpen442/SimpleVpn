using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SimpleVpn.Crypto
{
    //Handles Encrypting and Decrypting messages using AES-256
    public class Cipher
    {
        private string sharedKey { get; set; } // shared secret key
        private int keySize = 256; // use 256-bit AES key
        private int iterations = 4; // iterations to run PasswordDeriveBytes for

        private string hash = "SHA1";
        private string salt = "cb4BTrRzIMKLAUfa"; // REPLACE WITH RANDOMLY GENERATED VALUE TO BE SHARED ALONG WITH D-H VALUES
        private string IV = "nK9ATSb1pMy25zuc"; // REPLACE WITH RANDOMLY GENERATED VALUE TO BE SHARED ALONG WITH D-H VALUES

        public Cipher(string key)
        {
            sharedKey = key;
        }

        public byte[] Encrypt(IEnumerable<byte> plainText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + plainText.ByteArrToStr(), ConsoleColor.Cyan);

            CConsole.WriteLine("Encrypted To: " + Encrypt<AesManaged>(plainText, sharedKey).ByteArrToStr(), ConsoleColor.Yellow);

            return Encrypt<AesManaged>(plainText, sharedKey);
        }

        public byte[] Encrypt<T>(IEnumerable<byte> plainText, string password)
                where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = plainText.ToArray<byte>();
            byte[] saltBytes = ASCIIEncoding.ASCII.GetBytes(salt);
            byte[] vectorBytes = ASCIIEncoding.ASCII.GetBytes(IV);
            byte[] encrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, hash, iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }
            return encrypted;
        }

        public byte[] Decrypt(IEnumerable<byte> cipherText)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            CConsole.WriteLine("Decrypting Bytes: " + cipherText.ByteArrToStr(), ConsoleColor.Yellow);

            CConsole.WriteLine("Decrypted To: " + Decrypt<AesManaged>(cipherText, sharedKey).ByteArrToStr(), ConsoleColor.Cyan);

            return Decrypt<AesManaged>(cipherText, sharedKey);
        }

        public byte[] Decrypt<T>(IEnumerable<byte> cipherText, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = cipherText.ToArray<byte>();
            byte[] saltBytes = ASCIIEncoding.ASCII.GetBytes(salt);
            byte[] vectorBytes = ASCIIEncoding.ASCII.GetBytes(IV);
            byte[] decrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, hash, iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream from = new MemoryStream(valueBytes))
                        {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[valueBytes.Length];
                                reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    byte[] exceptionBytes = Encoding.ASCII.GetBytes(ex.ToString());
                    return exceptionBytes;
                }

                cipher.Clear();
            }

            return decrypted;
        }

        /*public byte[] Encrypt(IEnumerable<byte> cipherText)
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
        }*/
    }
}
