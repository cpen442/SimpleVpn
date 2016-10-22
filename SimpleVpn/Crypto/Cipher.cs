using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using SimpleVpn.Constants;

namespace SimpleVpn.Crypto
{
    //Handles Encrypting and Decrypting messages using AES-256
    public class Cipher
    {
        private byte[] key { get; set; } // shared secret session key obtained from Diffie-Hellman 
        private byte[] salt;
        private HMACSHA256 hmac;

        public Cipher(byte[] key)
        {
            salt = new byte[Variables.maxSaltIVLength];
            this.key = new byte[Variables.keySize];

            Buffer.BlockCopy(key, 0, this.salt, 0, Variables.maxSaltIVLength); //slice off first 16 bytes to be used for random salt for AES
            Buffer.BlockCopy(key, Variables.maxSaltIVLength, this.key, 0, Buffer.ByteLength(key) - (Variables.maxSaltIVLength));  // use remainder for session key   
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
            byte[] plainTextBytes = plainText.ToArray();
            byte[] encrypted;

            var IV = GenerateRandomCryptoValue();

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, Variables.hash, Variables.iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(Variables.keySize / 8);
                hmac = new HMACSHA256(keyBytes); //initiate HMAC

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
            var result = new List<byte>();
            result.AddRange(IV);
            result.AddRange(encrypted);
            result.AddRange(hmac.ComputeHash(result.ToArray()));
            return result.ToArray();
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
            int cipherTextLength = cipherText.Count() - Variables.maxSaltIVLength - Variables.hmacLength;
            byte[] ivPlusCipherTextBytes = cipherText.Take(Variables.maxSaltIVLength + cipherTextLength).ToArray();
            byte[] IV = cipherText.Take(Variables.maxSaltIVLength).ToArray();
            byte[] cipherTextBytes = cipherText.Skip(Variables.maxSaltIVLength).Take(cipherTextLength).ToArray();
            byte[] decrypted;
            
            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, Variables.hash, Variables.iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(Variables.keySize / 8);

                hmac = new HMACSHA256(keyBytes); //initiate HMAC
                byte[] hmacBytes = cipherText.Skip(Variables.maxSaltIVLength + cipherTextLength).ToArray();
                byte[] checkHMAC = hmac.ComputeHash(ivPlusCipherTextBytes);

                //Data Integrity Check
                if (!hmacBytes.SequenceEqual(checkHMAC))
                {
                    throw new UnauthorizedAccessException("HMAC Does Not Match. Data Tampering Detected. Closing Connection...");
                }

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

        //Helper Method to generate a random value to be used for salt or initialization vector (IV)
        private byte[] GenerateRandomCryptoValue()
        {
            byte[] result = new byte[Variables.maxSaltIVLength];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(result);
            }

            return result;
        }
    }
}
