using System;
using Timestamping;
using Org.BouncyCastle.Tsp;
using System.IO;
using Org.BouncyCastle.Math;
using System.Net;
using NUnit.Framework;

namespace TimestampTest
{
    [TestFixture]
    public class TimestampFileTest
    {

        readonly string cwd = TestContext.CurrentContext.WorkDirectory;

        public static byte[] GetHash()
        {
            String hash = "779A779AA7A8DE9BA34BAFF9702C99DC9B7EE061658A28EAD493FF2F1413DF02";
            byte[] hashByte = new byte[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(hash.Substring(i, 2), 16);
            }
            return hashByte;
        }

        public static TimeStampRequest CreateTsRequest()
        {
            byte[] hash = GetHash();
            TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();
            reqGen.SetCertReq(true);
            TimeStampRequest request = reqGen.Generate(TspAlgorithms.Sha256, hash, BigInteger.ValueOf(100));
            return request;
        }

        [Test, Category("Test")]
        public void CreateHash_WhenPathIsValid_ReturnsByteArray()
        {
            byte[] actual;
            using (Stream fs = new FileStream(cwd + @"\resources\file1.txt", FileMode.Open))
            {
                actual = TimestampFile.CreateHash(fs);
            }
            
            CollectionAssert.AllItemsAreNotNull(actual);
        }

        [Test, Category("Test")]
        public void CreateTimestampRequest_NonceNull_ReturnsTimestampRequestWithNonceNull()
        {
            byte[] hash = GetHash();
            BigInteger nonce = null;

            TimeStampRequest req = TimestampFile.CreateTimestampRequest(hash, true, nonce);

            Assert.IsNull(req.Nonce);
        }

        [Test, Category("Test")]
        public void CreateTimestampRequest_NonceGiven_ReturnsTimestampRequestWithCorrectNonce()
        {
            byte[] hash = GetHash();
            Random rnd = new Random();
            BigInteger nonce = new BigInteger(64, rnd);
            int expected = nonce.IntValue;

            TimeStampRequest req = TimestampFile.CreateTimestampRequest(hash, true, nonce);

            BigInteger nonceActual = req.Nonce;
            int actual = nonceActual.IntValue;
            Assert.AreEqual(actual, expected);
        }

        [Test, Category("Test")]
        public void CreateWebRequest_ShouldReturnWebRequest()
        {
            byte[] hash = GetHash();
            int expected = hash.Length;

            WebRequest req = TimestampFile.CreateWebRequest(hash, "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query");
            int actual = (int)req.ContentLength;

            Assert.AreEqual(expected, actual);
        }

        [Test, Category("Test")]
        public void CreateWebRequest_TimestampRequestDigestNull_ShouldThrowArgumentNullException()
        {
            TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();
            reqGen.SetCertReq(true);
            TimeStampRequest request = reqGen.Generate(TspAlgorithms.Sha256, null, BigInteger.ValueOf(100));
            Assert.Throws<ArgumentNullException>(() => TimestampFile.CreateWebRequest(request.GetEncoded(), "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query"));
        }

        [Test, Category("Test")]
        public void PostTimestampRequest_TimestampRequestValid_ReturnsWebResponseWithStatus200OK()
        {
            TimeStampRequest request = CreateTsRequest();
            double expected = 200;
            HttpWebRequest webReq = (HttpWebRequest)TimestampFile.CreateWebRequest(request.GetEncoded(), "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query");

            HttpWebResponse resp = (HttpWebResponse)TimestampFile.PostTimestampRequest(request, webReq);
            double actual = (double)resp.StatusCode;
            Assert.AreEqual(expected, actual, 0.1, "Failed Webresponse");
        }

        [Test, Category("Test")]
        public void CreateTimestampResponse_ValidWebResponse_ReturnsMatchingTimeStampResponse()
        {
            TimeStampRequest request = CreateTsRequest();
            HttpWebRequest webReq = (HttpWebRequest)TimestampFile.CreateWebRequest(request.GetEncoded(), "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query");
            HttpWebResponse resp = (HttpWebResponse)TimestampFile.PostTimestampRequest(request, webReq);

            TimeStampResponse response = TimestampFile.CreateTimestampResponse(resp);
            bool result = false;
            try
            {
                response.Validate(request);
                result = true;
            }
            catch { }

            Assert.IsTrue(result, "Response and Request cannot be matched");
        }

        [Test, Category("Test")]
        public void WriteTsResponseToStream_ShouldWriteResponseToGivenStream()
        {
            TimeStampRequest request = CreateTsRequest();
            HttpWebRequest webReq = (HttpWebRequest)TimestampFile.CreateWebRequest(request.GetEncoded(), "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query");
            HttpWebResponse resp = (HttpWebResponse)TimestampFile.PostTimestampRequest(request, webReq);
            TimeStampResponse response = TimestampFile.CreateTimestampResponse(resp);
            byte[] expected = response.GetEncoded();

            using (FileStream fs = new FileStream(cwd + @"\resources\response.tsr", FileMode.Create))
            {
                TimestampFile.WriteTsResponseToStream(response, fs);
            }
            byte[] actual = File.ReadAllBytes(cwd + @"\resources\response.tsr");

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test, Category("Test")]
        public void GetTimestamp_ValidPath_ShouldReturnValidTimestampResponseAndRequest()
        {
            String path = cwd + @"\resources\file1.txt";
            Tuple<TimeStampRequest, TimeStampResponse> result = TimestampFile.GetTimestamp(path, cwd + @"\resources\response.tsr", true, true);
            TimeStampRequest req = result.Item1;
            TimeStampResponse resp = result.Item2;

            bool valid = false;
            try
            {
                resp.Validate(req);
                valid = true;
            }
            catch { }

            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void GetTimestamp_WithValidHash_ShouldReturnValidTimestampResponseAndRequest()
        {
            byte[] hash = GetHash();
            Tuple<TimeStampRequest, TimeStampResponse> result = TimestampFile.GetTimestamp(hash, cwd + @"\resources\response.tsr", true, false);
            TimeStampRequest req = result.Item1;
            TimeStampResponse resp = result.Item2;

            bool valid = false;
            try
            {
                resp.Validate(req);
                valid = true;
            }
            catch { }

            Assert.IsTrue(valid);
        }
    }
}
