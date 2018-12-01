using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;

namespace GKNet.PGP
{
    public static class PGPUtilities
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

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

        public static Stream Streamify(this string theString, Encoding encoding = null)
        {
            return new MemoryStream((encoding ?? DefaultEncoding).GetBytes(theString));
        }

        public static string Stringify(this Stream theStream, Encoding encoding = null)
        {
            using (var reader = new StreamReader(theStream, encoding ?? DefaultEncoding)) {
                return reader.ReadToEnd();
            }
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
        ///     Signs the specified byte array using the specified key after unlocking the key with the specified passphrase.
        /// </summary>
        /// <param name="bytes">The byte array containing the payload to sign.</param>
        /// <param name="key">The PGP key to be used to sign the payload.</param>
        /// <param name="passphrase">The passphrase used to unlock the PGP key.</param>
        /// <returns>A byte array containing the generated PGP signature.</returns>
        public static byte[] Sign(byte[] bytes, string key, string passphrase)
        {
            // prepare a memory stream to hold the signature
            MemoryStream memoryStream = new MemoryStream();

            // prepare an armored output stream to produce an armored ASCII signature
            Stream outputStream = new ArmoredOutputStream(memoryStream);

            // retrieve the keys
            PgpSecretKey secretKey = ReadSecretKeyFromString(key);
            PgpPrivateKey privateKey = secretKey.ExtractPrivateKey(passphrase.ToCharArray());

            // create and initialize a signature generator
            PgpSignatureGenerator signatureGenerator = new PgpSignatureGenerator(secretKey.PublicKey.Algorithm, HashAlgorithmTag.Sha512);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, privateKey);

            // retrieve the first user id contained within the public key and use it to set the signature signer
            foreach (string userId in secretKey.PublicKey.GetUserIds()) {
                PgpSignatureSubpacketGenerator signatureSubpacketGenerator = new PgpSignatureSubpacketGenerator();
                signatureSubpacketGenerator.SetSignerUserId(false, userId);
                signatureGenerator.SetHashedSubpackets(signatureSubpacketGenerator.Generate());

                break;
            }

            // prepare a compressed data generator and compressed output stream to compress the data
            PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.ZLib);
            Stream compressedOutputStream = compressedDataGenerator.Open(outputStream);

            // generate the signature taken pretty much verbatim from the bouncycastle example; not sure what all of it does.
            BcpgOutputStream bcpgOutputStream = new BcpgOutputStream(compressedOutputStream);

            signatureGenerator.GenerateOnePassVersion(false).Encode(bcpgOutputStream);

            PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
            Stream literalOutputStream = literalDataGenerator.Open(bcpgOutputStream, PgpLiteralData.Binary, "signatureData", DateTime.UtcNow, new byte[4092]);

            foreach (byte b in bytes) {
                literalOutputStream.WriteByte(b);
                signatureGenerator.Update(b);
            }

            literalDataGenerator.Close();

            signatureGenerator.Generate().Encode(bcpgOutputStream);

            compressedDataGenerator.Close();

            outputStream.Close();

            // fetch a byte array containing the contents of the memory stream
            byte[] retVal = memoryStream.ToArray();

            // close the memory stream
            memoryStream.Close();

            // return the generated signature
            return retVal;
        }

        /// <summary>
        ///     Verifies the specified signature using the specified public key.
        /// </summary>
        /// <param name="input">A byte array containing the signature to verify.</param>
        /// <param name="publicKey">The public key with which to decode the signature.</param>
        /// <returns>A byte array containing the message contained within the signature.</returns>
        /// <exception cref="PgpDataValidationException">
        ///     Thrown when the specified signature is invalid, or if an exception is encountered while validating.
        /// </exception>
        public static byte[] Verify(byte[] input, string publicKey)
        {
            return Verify(Encoding.ASCII.GetString(input), publicKey);
        }

        /// <summary>
        ///     Verifies the specified signature using the specified public key.
        /// </summary>
        /// <param name="input">The signature to verify.</param>
        /// <param name="publicKey">The public key with which to decode the signature.</param>
        /// <returns>A byte array containing the message contained within the signature.</returns>
        /// <exception cref="PgpDataValidationException">
        ///     Thrown when the specified signature is invalid, or if an exception is encountered while validating.
        /// </exception>
        public static byte[] Verify(string input, string publicKey)
        {
            // create input streams from
            Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input ?? string.Empty));
            Stream publicKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(publicKey ?? string.Empty));

            // enclose all operations in a try/catch. if we encounter any exceptions verification fails.
            try {
                // lines taken pretty much verbatim from the bouncycastle example. not sure what it all does.
                inputStream = PgpUtilities.GetDecoderStream(inputStream);

                PgpObjectFactory pgpFact = new PgpObjectFactory(inputStream);
                PgpCompressedData c1 = (PgpCompressedData)pgpFact.NextPgpObject();
                pgpFact = new PgpObjectFactory(c1.GetDataStream());

                PgpOnePassSignatureList p1 = (PgpOnePassSignatureList)pgpFact.NextPgpObject();
                PgpOnePassSignature ops = p1[0];

                PgpLiteralData p2 = (PgpLiteralData)pgpFact.NextPgpObject();
                Stream dIn = p2.GetInputStream();
                PgpPublicKeyRingBundle pgpRing = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicKeyStream));
                PgpPublicKey key = pgpRing.GetPublicKey(ops.KeyId);

                // set up a memorystream to contain the message contained within the signature
                MemoryStream memoryStream = new MemoryStream();

                ops.InitVerify(key);

                int ch;
                while ((ch = dIn.ReadByte()) >= 0) {
                    ops.Update((byte)ch);
                    memoryStream.WriteByte((byte)ch);
                }

                // save the contents of the memorystream to a byte array
                byte[] retVal = memoryStream.ToArray();

                memoryStream.Close();

                PgpSignatureList p3 = (PgpSignatureList)pgpFact.NextPgpObject();
                PgpSignature firstSig = p3[0];

                // verify.
                if (ops.Verify(firstSig)) {
                    return retVal;
                } else {
                    throw new PgpDataValidationException();
                }
            } catch (Exception) {
                throw new PgpDataValidationException();
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
    }
}
