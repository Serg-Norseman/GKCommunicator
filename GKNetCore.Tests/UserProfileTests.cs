using System;
using System.Text;
using BencodeNET;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class UserProfileTests
    {
        [Test]
        public void Test_UserProfile_class()
        {
            UserProfile profile = new UserProfile();

            profile.Reset();

            Assert.IsFalse(profile.IsCountryVisible);
            profile.IsCountryVisible = true;
            Assert.IsTrue(profile.IsCountryVisible);

            Assert.IsFalse(profile.IsLanguagesVisible);
            profile.IsLanguagesVisible = true;
            Assert.IsTrue(profile.IsLanguagesVisible);

            Assert.IsFalse(profile.IsTimeZoneVisible);
            profile.IsTimeZoneVisible = true;
            Assert.IsTrue(profile.IsTimeZoneVisible);

            BDictionary data = new BDictionary();
            profile.Save(data);
            profile.Load(data);

            Assert.Throws(typeof(ArgumentNullException), () => { profile.Save(null); });
            Assert.Throws(typeof(ArgumentNullException), () => { profile.Load(null); });

            profile.Identify("password");
            Assert.IsFalse(string.IsNullOrEmpty(profile.PublicKey));
            Assert.IsFalse(string.IsNullOrEmpty(profile.PrivateKey));

            var result = Utilities.VerifyPassword("password", profile.PasswordHash);
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_RSAMessagesProtection()
        {
            string password = "password";

            string publicKey, privateKey;
            Utilities.GenerateKeyPair(password, out publicKey, out privateKey);
            Assert.AreEqual(243, publicKey.Length);
            Assert.AreEqual(1260, privateKey.Length);

            byte[] pubKey = Encoding.UTF8.GetBytes(publicKey);
            Assert.AreEqual(324, Convert.ToBase64String(pubKey).Length);

            // compressed keys: 272 (+12 %), 1304 (+3.5 %) -> do not use

            string inputText = "this is my test phrase!";

            string cryptoString = Utilities.Encrypt(inputText, publicKey);

            string outputString = Utilities.Decrypt(cryptoString, privateKey, password);
            Assert.AreEqual(inputText, outputString);
        }

        [Test]
        public void Test_Password()
        {
            var hash = Utilities.HashPassword("mypassword");
            var result = Utilities.VerifyPassword("mypassword", hash);
            Assert.IsTrue(result);
        }
    }
}
