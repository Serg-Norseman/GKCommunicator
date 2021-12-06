using System;
using System.Text;
using NUnit.Framework;

namespace BencodeNET
{
    [TestFixture]
    public class BencodeTests
    {
        [Test]
        public void Test_BNumber_class()
        {
            BNumber bnumber = new BNumber(12345);
            Assert.IsNotNull(bnumber);

            Assert.AreEqual(12345, bnumber.Value);
            Assert.AreEqual("i12345e", bnumber.EncodeAsString());

            Assert.IsTrue(bnumber.Equals((BNumber)12345));
            Assert.AreEqual(0, bnumber.CompareTo((BNumber)12345));

            int intVal = 123;
            bnumber = intVal;
            Assert.AreEqual(123, bnumber.Value);

            bnumber = 321;
            intVal = bnumber;
            Assert.AreEqual(321, intVal);

            long longVal = 456;
            bnumber = longVal;
            Assert.AreEqual(456, bnumber.Value);

            bnumber = 654;
            longVal = bnumber;
            Assert.AreEqual(654, longVal);

            bool boolVal = true;
            bnumber = boolVal;
            Assert.AreEqual(1, bnumber.Value);

            bnumber = 0;
            boolVal = bnumber;
            Assert.AreEqual(false, boolVal);
        }

        [Test]
        public void Test_BList_class()
        {
            BList blist = new BList();
            Assert.IsNotNull(blist);
            Assert.AreEqual("le", blist.EncodeAsString());

            IBObject obj = null;
            Assert.Throws(typeof(ArgumentNullException), () => { blist.Add(obj); });

            blist.Add("test");
            blist.Add(1234);
            Assert.AreEqual("l4:testi1234ee", blist.EncodeAsString());

            obj = blist.Get<BString>(0);
            Assert.AreEqual("test", obj.ToString());

            obj = blist.Get<BNumber>(1);
            Assert.AreEqual(1234, ((BNumber)obj).Value);

            var blistCopy = new BList(blist.Value);
            Assert.AreEqual("l4:testi1234ee", blistCopy.EncodeAsString());

            Assert.AreEqual(2, blistCopy.Count);
            blistCopy.Clear();
            Assert.AreEqual(0, blistCopy.Count);
        }

        [Test]
        public void Test_BDict_class()
        {
            BDictionary bdict = new BDictionary();
            Assert.IsNotNull(bdict);
            Assert.AreEqual("de", bdict.EncodeAsString());

            IBObject obj = null;
            Assert.Throws(typeof(ArgumentNullException), () => { bdict.Add("key", obj); });

            bdict.Add("str", "test");
            bdict.Add("number", 1234);
            Assert.AreEqual("d6:numberi1234e3:str4:teste", bdict.EncodeAsString());

            obj = bdict.Get<BString>("str");
            Assert.AreEqual("test", obj.ToString());

            obj = bdict.Get<BNumber>("number");
            Assert.AreEqual(1234, ((BNumber)obj).Value);

            Assert.AreEqual(2, bdict.Count);
            bdict.Clear();
            Assert.AreEqual(0, bdict.Count);
        }

        [Test]
        public void Test_BString_class()
        {
            string nullStr = null;
            Assert.Throws(typeof(ArgumentNullException), () => { new BString(nullStr); });

            byte[] nullBytes = null;
            Assert.Throws(typeof(ArgumentNullException), () => { new BString(nullBytes); });

            BString bstr = new BString("test");
            Assert.IsNotNull(bstr);

            Assert.AreEqual(new byte[] { (byte)'t', (byte)'e', (byte)'s', (byte)'t' }, bstr.Value);
            Assert.AreEqual(4, bstr.Length);
            Assert.AreEqual(Encoding.UTF8, bstr.Encoding);
            Assert.AreEqual("4:test", bstr.EncodeAsString());

            BString bstr2 = "implicit";
            Assert.AreEqual("8:implicit", bstr2.EncodeAsString());

            Assert.AreEqual(1, bstr.CompareTo(null));
            Assert.AreEqual(1, bstr.CompareTo(bstr2));
            Assert.AreEqual(0, bstr.CompareTo(bstr));

            Assert.IsFalse(bstr.Equals(bstr2));
            Assert.IsFalse(bstr.Equals((object)bstr2));

            object obj = null;
            Assert.IsFalse(bstr.Equals(obj));
        }

        [Test]
        public void Test_BencodeParser_class()
        {
            BencodeParser parser = new BencodeParser();
            Assert.IsNotNull(parser);

            IBObject bObj = parser.Parse("i12345e");
            Assert.IsInstanceOf<BNumber>(bObj);
            Assert.AreEqual(12345, ((BNumber)bObj).Value);

            bObj = parser.Parse("4:test");
            Assert.IsInstanceOf<BString>(bObj);
            Assert.AreEqual("test", ((BString)bObj).ToString());

            bObj = parser.Parse("l4:testi1234ee");
            Assert.IsInstanceOf<BList>(bObj);

            bObj = parser.Parse("d6:numberi1234e3:str4:teste");
            Assert.IsInstanceOf<BDictionary>(bObj);
        }
    }
}
