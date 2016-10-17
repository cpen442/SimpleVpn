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

        private const string hash = "SHA1";
        private const string salt = "cb4BTrRzIMKLAUfa"; // REPLACE WITH RANDOMLY GENERATED VALUE TO BE SHARED ALONG WITH D-H VALUES
        private const string IV = "nK9ATSb1pMy25zuc"; // REPLACE WITH RANDOMLY GENERATED VALUE TO BE SHARED ALONG WITH D-H VALUES

        private byte[] saltBytes;
        private byte[] vectorBytes;

        private int maxSaltIVLength = 32; // max length for randomly generated salt or IV values
        private int keySize = 256; // use 256-bit AES key
        private int iterations = 4; // iterations to run PasswordDeriveBytes for

        public Cipher(string key)
        {
            sharedKey = key;
            saltBytes = ASCIIEncoding.ASCII.GetBytes(salt);
            vectorBytes = ASCIIEncoding.ASCII.GetBytes(IV);
        }
        public Cipher(string key, byte[] salt, byte[] vector)
        {
            sharedKey = key;
            saltBytes = salt;
            vectorBytes = vector;
        }

        public byte[] Encrypt(IEnumerable<byte> plainText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + plainText.ByteArrToStr(), ConsoleColor.Cyan);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var enc = Encrypt<AesManaged>(plainText, sharedKey);

            CConsole.WriteLine("Encrypted To: " + enc.ByteArrToStr(), ConsoleColor.Yellow);
            return enc;
        }

        public byte[] Encrypt<T>(IEnumerable<byte> plainText, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = plainText.ToArray<byte>();
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

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var dec = Decrypt<AesManaged>(cipherText, sharedKey);

            CConsole.WriteLine("Decrypted To: " + dec.ByteArrToStr(), ConsoleColor.Cyan);

            return dec;
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
