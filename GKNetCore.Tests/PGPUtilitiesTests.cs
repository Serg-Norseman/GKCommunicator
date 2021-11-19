using BSLib;
using NUnit.Framework;

namespace GKNet
{
    public class PGPUtilitiesTests
    {
        [Test]
        public void Test_MessagesProtection()
        {
            string password = "password";

            string publicKey, privateKey;
            PGPUtilities.GenerateKey("John Doe", password, out publicKey, out privateKey);

            string inputText = "this is my test phrase!";

            string cryptoString = string.Empty;
            using (var streamPublKey = publicKey.Streamify()) {
                var publKey = streamPublKey.ImportPublicKey();

                using (var inputStream = inputText.Streamify())
                using (var cryptoStream = inputStream.PgpEncrypt(publKey)) {
                    cryptoString = cryptoStream.Stringify();
                }
            }

            using (var cryptoStream = cryptoString.Streamify())
            using (var outputStream = cryptoStream.PgpDecrypt(privateKey, password)) {
                string outputString = outputStream.Stringify();
                Assert.AreEqual(inputText, outputString);
            }
        }
    }
}
