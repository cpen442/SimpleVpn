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
        private byte[] key { get; set; } // shared secret session key obtained from Diffie-Hellman

        private string hash = "SHA1";
        private byte[] IV;
        private byte[] salt;
        private int maxSaltIVLength = 16; // max length for randomly generated salt or IV values
        private int keySize = 256; // use 256-bit AES key
        private int iterations = 4; // iterations to run PasswordDeriveBytes for

        public Cipher(byte[] key)
        {
            IV = new byte[maxSaltIVLength];
            salt = new byte[maxSaltIVLength];
            this.key = new byte[keySize];

            Buffer.BlockCopy(key, 0, IV, 0, maxSaltIVLength); //slice off first 16 bytes to be used for random IV for AES
            Buffer.BlockCopy(key, maxSaltIVLength, salt, 0, maxSaltIVLength); //slice off next 16 bytes to be used for random salt for AES
            Buffer.BlockCopy(key, maxSaltIVLength * 2, this.key, 0, Buffer.ByteLength(key) - (maxSaltIVLength * 2));  // use remainder for session key     
        }

        public byte[] Encrypt(IEnumerable<byte> plainText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + plainText.ByteArrToStr(), ConsoleColor.Cyan, nostep: true);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var res = Encrypt<AesManaged>(plainText, key);
            CConsole.WriteLine("Encrypted To: " + res.ByteArrToStr(), ConsoleColor.Yellow, nostep: true);

            return res;
        }

        public byte[] Encrypt<T>(IEnumerable<byte> plainText, byte[] password) where T : SymmetricAlgorithm, new()
        {
            byte[] plainTextBytes = plainText.ToArray<byte>();
            byte[] encrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, hash, iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, IV))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(plainTextBytes, 0, plainTextBytes.Length);
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
            CConsole.WriteLine("Decrypting Bytes: " + cipherText.ByteArrToStr(), ConsoleColor.Yellow, nostep: true);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var res = Decrypt<AesManaged>(cipherText, key);
            CConsole.WriteLine("Decrypted To: " + res.ByteArrToStr(), ConsoleColor.Cyan, nostep: true);

            return res;
        }

        public byte[] Decrypt<T>(IEnumerable<byte> cipherText, byte[] password) where T : SymmetricAlgorithm, new()
        {
            byte[] cipherTextBytes = cipherText.ToArray<byte>();
            byte[] decrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, hash, iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, IV))
                    {
                        using (MemoryStream from = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[cipherTextBytes.Length];
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
    }
}
