using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BSLib;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace GKNet
{
    public static class PGPUtilities
    {
        public static PgpPublicKey ImportPublicKey(this Stream publicIn, bool forEncryption = true)
        {
            return new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicIn)).
                GetKeyRings().OfType<PgpPublicKeyRing>().SelectMany(x => x.GetPublicKeys().OfType<PgpPublicKey>()).
                Where(key => !forEncryption || key.IsEncryptionKey).FirstOrDefault();
        }

        public static PgpSecretKey ImportSecretKey(this Stream secretIn)
        {
            return
            new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(secretIn))
            .GetKeyRings()
            .OfType<PgpSecretKeyRing>()
            .SelectMany(x => x.GetSecretKeys().OfType<PgpSecretKey>())
            .FirstOrDefault();
        }

        public static Stream PgpEncrypt(this Stream toEncrypt, PgpPublicKey encryptionKey, bool armor = true, bool verify = false)
        {
            var outStream = new MemoryStream();

            var encryptor = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, verify, new SecureRandom());
            var literalizer = new PgpLiteralDataGenerator();
            var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

            encryptor.AddMethod(encryptionKey);

            //it would be nice if these streams were read/write, and supported seeking.  Since they are not,
            //we need to shunt the data to a read/write stream so that we can control the flow of data as we go.

            using (var stream = new MemoryStream()) // this is the read/write stream
            using (var armoredStream = armor ? new ArmoredOutputStream(stream) : stream as Stream)
            using (var compressedStream = compressor.Open(armoredStream)) {
                //data is encrypted first, then compressed, but because of the one-way nature of these streams,
                //other "interim" streams are required.  The raw data is encapsulated in a "Literal" PGP object.
                var rawData = toEncrypt.ReadFully();
                var buffer = new byte[1024];

                using (var literalOut = new MemoryStream())
                using (var literalStream = literalizer.Open(literalOut, PgpLiteralData.Binary, "STREAM", DateTime.UtcNow, buffer)) {
                    literalStream.Write(rawData, 0, rawData.Length);
                    literalStream.Close();
                    var literalData = literalOut.ReadFully();

                    //The literal data object is then encrypted, which flows into the compressing stream and
                    //(optionally) into the ASCII armoring stream.
                    using (var encryptedStream = encryptor.Open(compressedStream, literalData.Length)) {
                        encryptedStream.Write(literalData, 0, literalData.Length);
                        encryptedStream.Close();
                        compressedStream.Close();
                        armoredStream.Close();

                        //the stream processes are now complete, and our read/write stream is now populated with
                        //encrypted data.  Convert the stream to a byte array and write to the out stream.
                        stream.Position = 0;
                        var data = stream.ReadFully();
                        outStream.Write(data, 0, data.Length);
                    }
                }
            }

            outStream.Position = 0;

            return outStream;
        }

        public static Stream PgpDecrypt(this Stream encryptedData, string armoredPrivateKey, string privateKeyPassword, Encoding armorEncoding = null)
        {
            var stream = PgpUtilities.GetDecoderStream(encryptedData);
            var layeredStreams = new List<Stream> { stream }; // this is to clean up/ dispose of any layered streams.
            var dataObjectFactory = new PgpObjectFactory(stream);
            var dataObject = dataObjectFactory.NextPgpObject();
            Dictionary<long, PgpSecretKey> secretKeys;

            using (var privateKeyStream = armoredPrivateKey.Streamify(armorEncoding ?? Encoding.UTF8))
            using (var decoderStream = PgpUtilities.GetDecoderStream(privateKeyStream)) {
                secretKeys = new PgpSecretKeyRingBundle(decoderStream).
                    GetKeyRings().OfType<PgpSecretKeyRing>().
                    SelectMany(x => x.GetSecretKeys().OfType<PgpSecretKey>()).
                    ToDictionary(key => key.KeyId, value => value);

                if (!secretKeys.Any())
                    throw new ArgumentException("No secret keys found.");
            }

            while (!(dataObject is PgpLiteralData) && dataObject != null) {
                try {
                    var compressedData = dataObject as PgpCompressedData;

                    var listedData = dataObject as PgpEncryptedDataList;

                    //strip away the compression stream
                    if (compressedData != null) {
                        stream = compressedData.GetDataStream();

                        layeredStreams.Add(stream);

                        dataObjectFactory = new PgpObjectFactory(stream);
                    }

                    //strip the PgpEncryptedDataList
                    if (listedData != null) {
                        var encryptedDataList =
                            listedData.GetEncryptedDataObjects().OfType<PgpPublicKeyEncryptedData>().First();

                        var decryptionKey =
                            secretKeys[encryptedDataList.KeyId].ExtractPrivateKey(privateKeyPassword.ToCharArray());

                        stream = encryptedDataList.GetDataStream(decryptionKey);

                        layeredStreams.Add(stream);

                        dataObjectFactory = new PgpObjectFactory(stream);
                    }

                    dataObject = dataObjectFactory.NextPgpObject();
                } catch (Exception ex) {
                    //Log exception here.
                    throw new PgpException("Failed to strip encapsulating streams.", ex);
                }
            }

            foreach (var layeredStream in layeredStreams) {
                layeredStream.Close();
                layeredStream.Dispose();
            }

            if (dataObject == null)
                return null;

            return (dataObject as PgpLiteralData).GetInputStream();
        }

        private static byte[] ReadFully(this Stream stream, int position = 0)
        {
            if (!stream.CanRead)
                throw new ArgumentException("This is not a readable stream.");

            if (stream.CanSeek)
                stream.Position = 0;

            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);

                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Reads and returns the PGP secret key from the specified input stream.
        /// </summary>
        /// <param name="input">The input stream containing the PGP secret key to be read.</param>
        /// <returns>The retrieved PGP secret key.</returns>
        private static PgpSecretKey ReadSecretKey(Stream input)
        {
            try {
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(input));
                PgpSecretKeyRing pgpKeyRing = pgpSec.GetKeyRings().OfType<PgpSecretKeyRing>().FirstOrDefault();
                PgpSecretKey pgpSecretKey = pgpKeyRing.GetSecretKeys().OfType<PgpSecretKey>().FirstOrDefault();

                if (pgpSecretKey.IsSigningKey) {
                    return pgpSecretKey;
                } else {
                    throw new Exception();
                }
            } catch (Exception) {
                throw new PgpKeyValidationException("Can't find a valid signing key in the specified key ring.");
            }
        }

        /// <summary>
        ///     Reads and returns the PGP secret key from the specified key string.
        /// </summary>
        /// <param name="key">The key string from which the PGP secret key is to be read.</param>
        /// <returns>The retrieved PGP secret key.</returns>
        private static PgpSecretKey ReadSecretKeyFromString(string key)
        {
            return ReadSecretKey(new MemoryStream(Encoding.UTF8.GetBytes(key ?? string.Empty)));
        }

        public static void GenerateKey(Stream publicKeyStream, Stream privateKeyStream, string username = null,
                                       string password = null, int strength = 1024, int certainty = 8, bool armor = true)
        {
            username = username == null ? string.Empty : username;
            password = password == null ? string.Empty : password;

            IAsymmetricCipherKeyPairGenerator kpg = new RsaKeyPairGenerator();
            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x13), new SecureRandom(), strength, certainty));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

            ExportKeyPair(privateKeyStream, publicKeyStream, kp.Public, kp.Private, username, password.ToCharArray(), armor);
        }

        public static void GenerateKey(string username, string password, out string publicKey, out string privateKey)
        {
            using (var publicKeyStream = new MemoryStream())
            using (var privateKeyStream = new MemoryStream()) {
                PGPUtilities.GenerateKey(publicKeyStream, privateKeyStream, username, password);

                publicKeyStream.Seek(0, SeekOrigin.Begin);
                privateKeyStream.Seek(0, SeekOrigin.Begin);

                publicKey = publicKeyStream.Stringify();
                privateKey = privateKeyStream.Stringify();
            }
        }

        private static void ExportKeyPair(Stream secretOut, Stream publicOut,
            AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey,
            string identity, char[] passPhrase, bool armor)
        {
            if (secretOut == null)
                throw new ArgumentException("secretOut");
            if (publicOut == null)
                throw new ArgumentException("publicOut");

            if (armor) {
                secretOut = new ArmoredOutputStream(secretOut);
            }

            PgpSecretKey secretKey = new PgpSecretKey(
                                         PgpSignature.DefaultCertification, PublicKeyAlgorithmTag.RsaGeneral,
                                         publicKey, privateKey, DateTime.Now, identity,
                                         SymmetricKeyAlgorithmTag.TripleDes, passPhrase, null, null, new SecureRandom());

            secretKey.Encode(secretOut);

            secretOut.Dispose();

            if (armor) {
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;

            key.Encode(publicOut);

            publicOut.Dispose();
        }
    }
}
