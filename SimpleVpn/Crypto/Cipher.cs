using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using SimpleVpn.Constants;

namespace SimpleVpn.Crypto
{
    //This class handles Encrypting and Decrypting messages using AES-256

    public class Cipher
    {
        /* fields */
        private byte[] encryptionKey { get; set; } // shared secret session key obtained from Diffie-Hellman
        private byte[] salt;

        /* constructor */
        public Cipher(byte[] key)
        {
            salt = new byte[Variables.maxSaltIVLength];
            this.encryptionKey = new byte[Variables.AESkeySize];

            // slice off next 16 bytes to be used for random salt for AES
            Buffer.BlockCopy(key, 0, this.salt, 0, Variables.maxSaltIVLength);

            // use remainder for session key  
            Buffer.BlockCopy(key, Variables.maxSaltIVLength, this.encryptionKey, 0, Buffer.ByteLength(key) - (Variables.maxSaltIVLength));
        }



        /* encrypt */
        public byte[] Encrypt(IEnumerable<byte> plainText)
        {
            CConsole.WriteLine("Encrypting Bytes: " + plainText.ByteArrToStr(), ConsoleColor.Cyan, nostep: true);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var res = Encrypt<AesManaged>(plainText, encryptionKey);
            CConsole.WriteLine("Encrypted To: " + res.ByteArrToStr(), ConsoleColor.Yellow, nostep: true);

            return res;
        }

        /* encrypt helper: implements block cipher with integrity check*/
        public byte[] Encrypt<T>(IEnumerable<byte> plainText, byte[] password) where T : SymmetricAlgorithm, new()
        {
            byte[] plainTextBytes = plainText.ToArray<byte>();
            byte[] encrypted;
            byte[] integrityKeyBytesOut;


            var IV = GenerateRandomCryptoValue();

            using (T cipher = new T())
            {
                // get key bytes from session key, salt, hash algorithm, and num iterations
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, Variables.hash, Variables.iterations);
                byte[] encryptionKeyBytes = _passwordBytes.GetBytes((Variables.AESkeySize) / 8);
                byte[] integrityKeyBytes = _passwordBytes.GetBytes((Variables.HMACkeySize) / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(encryptionKeyBytes, IV))
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

                integrityKeyBytesOut = integrityKeyBytes;
                cipher.Clear();
            }


            // result is IV + encrypted bytes
            byte[] result = ADD(IV, encrypted);

            // integrity key is HMAC(result, integrity key)
            byte[] MAC = HMAC(integrityKeyBytesOut, result);

            // result is MAC + IV + ciphertext
            result = ADD(MAC, result);

            return result.ToArray();
        }




        /* decrypt */
        public byte[] Decrypt(IEnumerable<byte> cipherText)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            CConsole.WriteLine("Decrypting Bytes: " + cipherText.ByteArrToStr(), ConsoleColor.Yellow, nostep: true);

            //May use SymmetricAlgorithms other than AesManaged if desired, e.g. RijndaelManaged
            var res = Decrypt<AesManaged>(cipherText, encryptionKey);
            CConsole.WriteLine("Decrypted To: " + res.ByteArrToStr(), ConsoleColor.Cyan, nostep: true);

            return res;
        }

        /* decrypt helper: block cipher with integrity check*/
        public byte[] Decrypt<T>(IEnumerable<byte> cipherText, byte[] password) where T : SymmetricAlgorithm, new()
        {
            // get values from cipher: (MAC + IV + ciphertext)
            byte[] receivedMAC = cipherText.Take(Variables.HMACkeySize).ToArray();
            byte[] result = cipherText.Skip(Variables.HMACkeySize).ToArray<byte>();

            byte[] IV = result.Take(Variables.maxSaltIVLength).ToArray();
            byte[] cipherTextBytes = result.Skip(Variables.maxSaltIVLength).ToArray<byte>();

            /*byte[] IV = cipherText.Take(Variables.maxSaltIVLength).ToArray();
            byte[] cipherTextBytes = cipherText.Skip(Variables.maxSaltIVLength).ToArray<byte>();*/

            byte[] decrypted;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, salt, Variables.hash, Variables.iterations);
                byte[] decryptionKeyBytes = _passwordBytes.GetBytes(Variables.AESkeySize / 8);
                byte[] integrityKeyBytes = _passwordBytes.GetBytes(Variables.HMACkeySize / 8);

                // compute MAC
                //byte[] computedMAC = HMAC(integrityKeyBytes, result);

                // check MAC
                //if (!computedMAC.Equals(receivedMAC))
               //     throw new UnauthorizedAccessException("Unauthorized message: MAC values do not match");

                // begin decryption process
                cipher.Mode = CipherMode.CBC;
                
                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(decryptionKeyBytes, IV))
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
                    // catch unauthorized msgs
                    byte[] exceptionBytes = Encoding.ASCII.GetBytes(ex.ToString());
                    return exceptionBytes;
                }

                cipher.Clear();
            }

            return decrypted;
        }


        /*Helper Methods*/

        //generate a random value to be used for salt or initialization vector (IV)
        private byte[] GenerateRandomCryptoValue()
        {
            byte[] result = new byte[Variables.maxSaltIVLength];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(result);
            }

            return result;
        }

        //compute the HMAC SHA256 value
        private static byte[] HMAC(byte[] key, byte[] message)
        {
            var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(message);
        }

        // concatenate 2 byte arrays" first|second
        public static byte[] ADD(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);

            return result;
        }
    }
}
