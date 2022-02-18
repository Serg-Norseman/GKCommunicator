using System;
using System.Text;
using SCC = System.Security.Cryptography;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Encryption algorithm SHA256.
    /// </summary>
    public class SHA256 : IAlgorithm
    {
        private SCC.SHA256 fSHA256 = null;

        public SHA256()
        {
            fSHA256 = SCC.SHA256.Create();
        }

        public string GetHash(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var hashByte = fSHA256.ComputeHash(bytes);
            var hash = BitConverter.ToString(hashByte);

            var formattedHash = hash.Replace("-", "").ToLower();
            return formattedHash;
        }

        public string GetHash(IHashable data)
        {
            var dataBeforeHash = data.GetHashableContent();
            var hash = GetHash(dataBeforeHash);
            return hash;
        }

        public override string ToString()
        {
            return "SHA 256";
        }
    }
}
