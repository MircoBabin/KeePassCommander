using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KeePassCommander
{
    public class Encryption
    {
        //Diffie–Hellman key exchange establishes a shared secret between two parties
        //that can be used for secret communication for exchanging data over a public network. 

        private Encoding utf8WithoutBom = new UTF8Encoding(true);
        private ECDiffieHellmanCng DiffieHellmanMerkle;
        public byte[] PublicKeyForSettlement { get { return DiffieHellmanMerkle.PublicKey.ToByteArray(); } }

        public byte[] SharedKey;

        public Encryption()
        {
            DiffieHellmanMerkle = new ECDiffieHellmanCng(521);
            DiffieHellmanMerkle.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            DiffieHellmanMerkle.HashAlgorithm = CngAlgorithm.Sha512;

            SharedKey = null;
        }

        public void SettleSharedKey(byte[] otherPublicKeyForSettlement)
        {
            CngKey otherKey = CngKey.Import(otherPublicKeyForSettlement, CngKeyBlobFormat.EccPublicBlob);
            SharedKey = DiffieHellmanMerkle.DeriveKeyMaterial(otherKey);
        }

        private void SetAesKey(Aes aes)
        {
            byte[] salt = new byte[8];
            Buffer.BlockCopy(aes.IV, 0, salt, 0, 8);

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(SharedKey, salt, 1000); // Uses HMAC-SHA1

            aes.Key = pbkdf2.GetBytes(aes.KeySize / 8);
        }

        public byte[] Encrypt(string data)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream outputBytes = new MemoryStream())
                {
                    aes.GenerateIV();

                    byte[] bytes = BitConverter.GetBytes((UInt32)aes.IV.Length);
                    if (bytes.Length != 4) throw new Exception("BitConverter.GetBytes(UInt32) must return 4 bytes.");
                    outputBytes.Write(bytes, 0, 4);
                    outputBytes.Write(aes.IV, 0, aes.IV.Length);

                    SetAesKey(aes);

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        using (CryptoStream cs = new CryptoStream(outputBytes, encryptor, CryptoStreamMode.Write))
                        {
                            var plainBytes = utf8WithoutBom.GetBytes(data);
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.Close();
                            return outputBytes.ToArray();
                        }
                    }
                }
            }
        }

        public string Decrypt(byte[] data)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream inputBytes = new MemoryStream(data))
                {
                    byte[] bytes;

                    bytes = new byte[4];
                    inputBytes.Read(bytes, 0, 4);
                    UInt32 IVLength = BitConverter.ToUInt32(bytes, 0);
                    if (IVLength != (aes.BlockSize / 8)) 
                        throw new Exception("Bytesize of IV must match the blocksize " + (aes.BlockSize / 8) + ", but is " + IVLength + ".");

                    bytes = new byte[IVLength];
                    inputBytes.Read(bytes, 0, (int)IVLength);
                    aes.IV = bytes;

                    SetAesKey(aes);

                    using (MemoryStream outputBytes = new MemoryStream())
                    {
                        using (var decryptor = aes.CreateDecryptor())
                        {
                            using (CryptoStream cs = new CryptoStream(outputBytes, decryptor, CryptoStreamMode.Write))
                            {
                                inputBytes.CopyTo(cs);
                                cs.Close();

                                return utf8WithoutBom.GetString(outputBytes.ToArray());
                            }
                        }
                    }
                }
            }
        }

        public static void SecureDelete(string filename)
        {
            // Not really needed, because the file contents are already encrypted.

            if (!File.Exists(filename)) return;

            try
            {
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    var length = stream.Length;
                    byte[] block = new byte[256] {
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    };

                    long blockcount = (length / block.Length) + (length % block.Length == 0 ? 0 : 1);
                    for (var i = 0; i < blockcount; i++)
                    {
                        stream.Write(block, 0, block.Length);
                    }

                    stream.Flush();
                }
            }
            catch { }

            try
            {
                File.Delete(filename);
            }
            catch { }
        }
    }
}
