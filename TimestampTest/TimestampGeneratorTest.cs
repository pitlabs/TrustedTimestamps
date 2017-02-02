using System;
using NUnit;
using NUnit.Framework;
using TimestampMain;

namespace TimestampTest
{
    [TestFixture]
    public class TimestampGeneratorTest
    {
        [Test]
        public void CreateTimestamp_WithStringOfHash_ShouldReturnNonEmptyByteArray()
        {
            TimestampGenerator tsGen = new TimestampGenerator();
            byte[] received = tsGen.CreateTimestamp("78AFF182967B1CA294099FC9E49BCEC10FF39883047666A55E5BD10950F07508");
            CollectionAssert.IsNotEmpty(received);
        }
    }
}
