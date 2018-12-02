using System;
using System.Linq;
using System.Net;
using LumiSoft.Net.STUN.Client;
using LumiSoft.Net.STUN.Message;
using NUnit.Framework;

namespace LumiSoft.Net.STUN
{
    [TestFixture]
    public class STUNTests
    {
        [Test]
        public void Test_STUN_Result_ctor()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 8000);
            var stunResult = new STUN_Result(STUN_NetType.PortRestrictedCone, endPoint);
            Assert.AreEqual(STUN_NetType.PortRestrictedCone, stunResult.NetType);
            Assert.AreEqual(endPoint, stunResult.PublicEndPoint);
        }

        [Test]
        public void Test_STUN_ErrorCode_tor()
        {
            var errorCode = new STUN_ErrorCode(111, "test");
            Assert.AreEqual(111, errorCode.Code);
            Assert.AreEqual("test", errorCode.ReasonText);
        }

        [Test]
        public void Test_STUN_ChangeRequest_ctor()
        {
            var changeRequest = new STUN_ChangeRequest();
            Assert.AreEqual(true, changeRequest.ChangeIP);
            Assert.AreEqual(true, changeRequest.ChangePort);
        }

        [Test]
        public void Test_STUN_ChangeRequest_ctor2()
        {
            var changeRequest = new STUN_ChangeRequest(true, false);
            Assert.AreEqual(true, changeRequest.ChangeIP);
            Assert.AreEqual(false, changeRequest.ChangePort);
        }
    }
}
