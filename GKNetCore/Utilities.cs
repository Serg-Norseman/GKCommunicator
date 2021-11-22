/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GKNet
{
    public static class Utilities
    {
        private static readonly Regex ValidIpAddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]):[0-9]+$");

        public static bool IsValidIpAddress(string address)
        {
            return ValidIpAddressRegex.IsMatch(address);
        }

        public static string GetAppPath()
        {
            Module[] mods = Assembly.GetExecutingAssembly().GetModules();
            string fn = mods[0].FullyQualifiedName;
            return Path.GetDirectoryName(fn) + Path.DirectorySeparatorChar;
        }

        public static void LoadExtFile(string fileName)
        {
            if (File.Exists(fileName)) {
                Process.Start(new ProcessStartInfo("file://" + fileName) { UseShellExecute = true });
            } else {
                Process.Start(fileName);
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

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
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] encrypted;

            using (Aes aes = Aes.Create()) {
                try {
                    aes.Key = passwordHash;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.ECB;
                    aes.CreateEncryptor(aes.Key, aes.IV);

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                        using (MemoryStream to = new MemoryStream()) {
                            to.Write(aes.IV, 0, 16);
                            using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write)) {
                                writer.Write(contentBytes, 0, contentBytes.Length);
                                writer.FlushFinalBlock();
                                encrypted = to.ToArray();
                            }
                        }
                    }
                } finally {
                    aes.Clear();
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptPrivateKey(string content, string password)
        {
            byte[] passwordHash = GenerateHash(password);
            byte[] contentBytes = Convert.FromBase64String(content);

            byte[] decrypted;
            int decryptedByteCount = 0;

            byte[] initialVector = new byte[16];
            using (MemoryStream from = new MemoryStream(contentBytes)) {
                from.Read(initialVector, 0, 16);

                using (Aes aes = Aes.Create()) {
                    try {
                        aes.Key = passwordHash;
                        aes.IV = initialVector;
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
                    } finally {
                        aes.Clear();
                    }
                }
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

        private static byte[] GenerateHash(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            using (SHA256 sha = SHA256.Create()) {
                return sha.ComputeHash(passwordBytes);
            }
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

        private const int SaltSize = 16;
        private const int HashSize = 20;

        public static string HashPassword(string password, int iterations = 10000)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations)) {
                hash = pbkdf2.GetBytes(HashSize);
            }

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            var base64Hash = Convert.ToBase64String(hashBytes);
            return string.Format("$GKH$V1${0}${1}", iterations, base64Hash);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (!hashedPassword.Contains("$GKH$V1$")) {
                throw new NotSupportedException("The hashtype is not supported");
            }

            var splittedHashString = hashedPassword.Replace("$GKH$V1$", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];
            var hashBytes = Convert.FromBase64String(base64Hash);

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations)) {
                hash = pbkdf2.GetBytes(HashSize);
            }

            for (var i = 0; i < HashSize; i++) {
                if (hashBytes[i + SaltSize] != hash[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}
