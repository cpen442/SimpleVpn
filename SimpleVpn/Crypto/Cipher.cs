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
        private string key { get; set; } // shared secret session key obtained from Diffie-Hellman

        private string hash = "SHA1";
        private string salt;
        private string IV;
        private int maxSaltIVLength = 16; // max length for randomly generated salt or IV values
        private int keySize = 256; // use 256-bit AES key
        private int iterations = 4; // iterations to run PasswordDeriveBytes for

        public Cipher(string key)
        {
            IV = key.Substring(0, maxSaltIVLength); //slice off first 16 bytes to be used for random IV for AES
            salt = key.Substring(maxSaltIVLength, maxSaltIVLength); //slice off next 16 bytes to be used for random salt for AES

            this.key = key.Substring(maxSaltIVLength * 2); //use remainder as shared secret session key
        }

        public byte[] Encrypt(IEnumerable<byte> plainText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + plainText.ByteArrToStr(), ConsoleColor.Cyan);

            CConsole.WriteLine("Encrypted To: " + Encrypt<AesManaged>(plainText, key).ByteArrToStr(), ConsoleColor.Yellow);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            return Encrypt<AesManaged>(plainText, key);
        }

        public byte[] Encrypt<T>(IEnumerable<byte> plainText, string password) where T : SymmetricAlgorithm, new()
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

            CConsole.WriteLine("Decrypted To: " + Decrypt<AesManaged>(cipherText, key).ByteArrToStr(), ConsoleColor.Cyan);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            return Decrypt<AesManaged>(cipherText, key);
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

        //Helper Method to generate a random value to be used for salt or initialization vector (IV)
        private string GenerateRandomCryptoValue()
        {
            byte[] result = new byte[maxSaltIVLength];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(result);
            }

            return result.ByteArrToStr();
        }
    }
}
