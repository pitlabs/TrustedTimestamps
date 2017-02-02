using System;
using NUnit.Framework;
using TimestampMain;

namespace TimestampTest
{
    [TestFixture]
    public class TimestampVerifyerTest
    {
        [Test]
        public void GetVerification_ValidByteArrayAndOriginalHash_ShouldReturnTrue()
        {
            string hash = "78AFF182967B1CA294099FC9E49BCEC10FF39883047666A55E5BD10950F07508";
            TimestampGenerator tsGen = new TimestampGenerator();
            tsGen.CertReq = true;
            byte[] ts = tsGen.CreateTimestamp(hash);
            TimestampVerifyer tsVerify = new TimestampVerifyer();
            Assert.IsTrue(tsVerify.GetVerification(ts, hash));
        }
    }
}
