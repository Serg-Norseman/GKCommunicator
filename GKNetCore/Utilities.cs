using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace GKNet
{
    public static class Utilities
    {
        /// <summary>
        /// Compresses a string and returns a deflate compressed, Base64 encoded string.
        /// </summary>
        /// <param name="uncompressedString">String to compress</param>
        public static string Compress(string uncompressedString)
        {
            byte[] compressedBytes;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString))) {
                using (var compressedStream = new MemoryStream()) {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionMode.Compress, true)) {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }

        /// <summary>
        /// Decompresses a deflate compressed, Base64 encoded string and returns an uncompressed string.
        /// </summary>
        /// <param name="compressedString">String to decompress.</param>
        public static string Decompress(string compressedString)
        {
            byte[] decompressedBytes;

            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {
                using (var decompressedStream = new MemoryStream()) {
                    decompressorStream.CopyTo(decompressedStream);

                    decompressedBytes = decompressedStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }

        private static string EncryptPrivateKey(string content, string password)
        {
            byte[] passwordHash = GenerateHash(password);
            byte[] valueBytes = Encoding.UTF8.GetBytes(content);
            byte[] encrypted;

            using (Aes aes = Aes.Create()) {
                aes.Key = passwordHash;
                aes.GenerateIV();
                aes.Mode = CipherMode.ECB;
                aes.CreateEncryptor(aes.Key, aes.IV);

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    using (MemoryStream to = new MemoryStream()) {
                        to.Write(aes.IV, 0, 16);
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write)) {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                aes.Clear();
            }

            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptPrivateKey(string content, string password)
        {
            byte[] passwordHash = GenerateHash(password);
            byte[] contentBytes = Convert.FromBase64String(content);

            byte[] decrypted;
            int decryptedByteCount = 0;

            byte[] _initialVector = new byte[16];
            using (MemoryStream from = new MemoryStream(contentBytes)) {
                from.Read(_initialVector, 0, 16);

                using (Aes aes = Aes.Create()) {
                    aes.Key = passwordHash;
                    aes.IV = _initialVector;
                    aes.Mode = CipherMode.ECB;
                    aes.CreateEncryptor(aes.Key, aes.IV);

                    try {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read)) {
                                decrypted = new byte[content.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    } catch (Exception e) {
                        return string.Empty;
                    }
                    aes.Clear();
                }
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

        private static byte[] GenerateHash(string password)
        {
            SHA256 sha = SHA256Managed.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            return sha.ComputeHash(passwordBytes);
        }

        public static void GenerateKeyPair(string password, out string publicKey, out string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(1024)) {
                try {
                    publicKey = rsa.ToXmlString(false);
                    privateKey = EncryptPrivateKey(rsa.ToXmlString(true), password);
                } finally {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static string Encrypt(string content, string publicKey)
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
                rsa.FromXmlString(publicKey);
                return Convert.ToBase64String(rsa.Encrypt(contentBytes, true));
            }
        }

        public static string Decrypt(string content, string privateKey, string password)
        {
            string decryptedPrivateKey = DecryptPrivateKey(privateKey, password);
            byte[] contentBytes = Convert.FromBase64String(content);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
                rsa.FromXmlString(decryptedPrivateKey);
                return Encoding.UTF8.GetString(rsa.Decrypt(contentBytes, true));
            }
        }
    }
}
