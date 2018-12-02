using System;
using System.Linq;
using BencodeNET;
using GKNet;
using NUnit.Framework;

namespace GKNet
{
    [TestFixture]
    public class UserProfileTests
    {
        [Test]
        public void UserProfile_Tests()
        {
            UserProfile profile = new UserProfile();

            profile.ResetSystem();

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

            profile.GenerateKey("user@email.com", "password");
            Assert.IsNotNullOrEmpty(profile.PublicKey);
            Assert.IsNotNullOrEmpty(profile.PrivateKey);
        }
    }
}
