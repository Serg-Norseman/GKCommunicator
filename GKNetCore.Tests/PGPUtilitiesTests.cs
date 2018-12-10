using System;
using System.IO;
using System.Text;
using BSLib;
using NUnit.Framework;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace GKNet
{
    public class PGPUtilitiesTests
    {
        private const string PrivKeyPassword = "8&$gy$8rrPO^tbE1m5";
        private string privKey, publKey, newPublicKey;

        public PGPUtilitiesTests()
        {
            privKey = File.ReadAllText(Path.Combine("Keys", "privateKey.asc"));
            publKey = File.ReadAllText(Path.Combine("Keys", "publicKey.asc"));
            newPublicKey = File.ReadAllText(Path.Combine("Keys", "newPublicKey.asc"));
        }

        [Test]
        [TestCase("this is my test phrase!")]
        [TestCase("I can't believe it's not butter!")]
        [TestCase("abcjwnljkgnoingk54jngkj43n98thtion34klgnerkolnvION!OInolgkn34lkgn34oklngi34hngio435n")]
        public void Verify_Encryption_Output(string input)
        {
            string cryptoString = string.Empty;

            using (var stream = publKey.Streamify()) {
                var key = stream.ImportPublicKey();

                using (var clearStream = input.Streamify())
                using (var cryptoStream = clearStream.PgpEncrypt(key)) {
                    cryptoString = cryptoStream.Stringify();
                }
            }

            using (var stream = cryptoString.Streamify())
            using (var clearStream = stream.PgpDecrypt(privKey, PrivKeyPassword)) {
                Assert.AreEqual(input, clearStream.Stringify());
            }
        }

        [Test]
        public void Sign()
        {
            string text = "hello world!";

            byte[] bytes = Encoding.ASCII.GetBytes(text);
            byte[] signatureBytes = PGPUtilities.Sign(bytes, privKey, PrivKeyPassword);

            string signature = Encoding.ASCII.GetString(signatureBytes);

            Assert.NotNull(signature);
            Assert.AreNotEqual(string.Empty, signature);
        }

        [Test]
        public void SignBadPassword()
        {
            Assert.Throws(typeof(PgpException), () => { PGPUtilities.Sign(new byte[0], privKey, string.Empty); });
        }

        [Test]
        public void SignBlankKey()
        {
            Assert.Throws(typeof(PgpKeyValidationException), () => { PGPUtilities.Sign(new byte[0], string.Empty, PrivKeyPassword); });
        }

        [Test]
        public void SignNullBytes()
        {
            Assert.Throws(typeof(NullReferenceException), () => { PGPUtilities.Sign(null, privKey, PrivKeyPassword); });
        }

        [Test]
        public void SignNullKey()
        {
            Assert.Throws(typeof(PgpKeyValidationException), () => { PGPUtilities.Sign(new byte[0], null, PrivKeyPassword); });
        }

        [Test]
        public void SignNullPassword()
        {
            Assert.Throws(typeof(NullReferenceException), () => { PGPUtilities.Sign(new byte[0], privKey, null); });
        }

        [Test]
        public void SignZeroBytes()
        {
            byte[] signature = PGPUtilities.Sign(new byte[0], privKey, PrivKeyPassword);

            Assert.NotNull(Encoding.ASCII.GetString(signature));
        }

        [Test]
        public void Verify()
        {
            string text = "hello world!";
            byte[] signature = GetSignature(text);

            Assert.NotNull(signature);
            Assert.AreNotEqual(0, signature.Length);

            byte[] message = PGPUtilities.Verify(signature, publKey);

            Assert.NotNull(message);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual(text, Encoding.ASCII.GetString(message));
        }

        [Test]
        public void VerifyBadKey()
        {
            string text = "hello world!";
            byte[] signature = GetSignature(text);

            Assert.NotNull(signature);
            Assert.AreNotEqual(0, signature.Length);

            Assert.Throws(typeof(PgpDataValidationException), () => { PGPUtilities.Verify(signature, string.Empty); });
        }

        [Test]
        public void VerifyBadSignature()
        {
            Assert.Throws(typeof(PgpDataValidationException), () => { PGPUtilities.Verify(new byte[0], publKey); });
        }

        [Test]
        public void VerifyNulls()
        {
            string str = null;
            Assert.Throws(typeof(PgpDataValidationException), () => { PGPUtilities.Verify(str, null); });
        }

        [Test]
        public void VerifyString()
        {
            string text = "hello again world!";
            string signature = GetSignatureString(text);

            Assert.NotNull(signature);
            Assert.AreNotEqual(string.Empty, signature);

            byte[] message = PGPUtilities.Verify(signature, publKey);

            Assert.NotNull(message);
            Assert.AreNotEqual(0, message.Length);
            Assert.AreEqual(text, Encoding.ASCII.GetString(message));
        }

        [Test]
        public void VerifyWrongKey()
        {
            string text = "hello world!";
            byte[] signature = GetSignature(text);

            Assert.NotNull(signature);
            Assert.AreNotEqual(0, signature.Length);

            Assert.Throws(typeof(PgpDataValidationException), () => { PGPUtilities.Verify(signature, newPublicKey); });
        }

        /// <summary>
        ///     Returns a PGP signature of the specified text using the default private key.
        /// </summary>
        /// <param name="text">The text for which the signature will be generated.</param>
        /// <returns>The generated PGP signature.</returns>
        private byte[] GetSignature(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            return PGPUtilities.Sign(bytes, privKey, PrivKeyPassword);
        }

        /// <summary>
        ///     Returns a PGP signature, in the form of a string, of the specified text using the default private key.
        /// </summary>
        /// <param name="text">The text for which the signature will be generated.</param>
        /// <returns>The generated PGP signature, in the form of a string.</returns>
        private string GetSignatureString(string text)
        {
            return Encoding.ASCII.GetString(GetSignature(text));
        }

        [Test]
        public void GenerateKey()
        {
            using (var publicKeyStream = new MemoryStream())
            using (var privateKeyStream = new MemoryStream()) {
                //PGPUtilities.GenerateKey(@"C:\TEMP\keys\public.asc", @"C:\TEMP\keys\private.asc", "email@email.com", "password");
                PGPUtilities.GenerateKey(publicKeyStream, privateKeyStream, "email@email.com", "password");
            }
        }
    }
}
