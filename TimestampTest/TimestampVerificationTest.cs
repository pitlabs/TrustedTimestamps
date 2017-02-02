using System;
using Verify;
using Timestamping;
using Org.BouncyCastle.Tsp;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Org.BouncyCastle.Math;
using NUnit.Framework;

namespace TimestampTest
{
    [TestFixture]
    public class TimestampVerificationTest
    {
        readonly string cwd = TestContext.CurrentContext.WorkDirectory;

        public TimeStampResponse GetResponse()
        {
            String hash = "779A779AA7A8DE9BA34BAFF9702C99DC9B7EE061658A28EAD493FF2F1413DF02";
            hash.ToLower();
            byte[] hashByte = new byte[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(hash.Substring(i, 2), 16);
            }
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hashByte, cwd + @"\resources\response.tsr", true, false);
            return ts.Item2;
        }

        public TimeStampResponse GetResponseWithoutCert()
        {
            String hash = "779A779AA7A8DE9BA34BAFF9702C99DC9B7EE061658A28EAD493FF2F1413DF02";
            hash.ToLower();
            byte[] hashByte = new byte[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(hash.Substring(i, 2), 16);
            }
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hashByte, cwd + @"\resources\response.tsr", false, false);
            return ts.Item2;
        }

        public Tuple<TimeStampRequest, TimeStampResponse> GetTuple()
        {
            String hash = "9e779dd164355d8116fb6daa6fcb651ed20ef33c6bccc3455cd375c027cefeca";
            byte[] hashByte = new byte[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(hash.Substring(i, 2), 16);
            }
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hashByte, cwd + @"\resources\response.tsr", true, false);
            return ts;
        }

        [Test, Category("Test")]
        public void GetTsaCert_ValidTimeStampResponse_ShouldReturnValidX509Certificate()
        {
            TimeStampResponse resp = GetResponse();
            Org.BouncyCastle.X509.X509Certificate cert = TimestampVerification.GetTsaCert(resp.TimeStampToken);
            bool isValid = cert.IsValid(DateTime.Now);
            Assert.IsTrue(isValid);
        }

        [Test, Category("Test")]
        public void GetTsaCert_TimeStampResponseWithoutCertificate_ShouldThrowTspException()
        {
            TimeStampResponse resp = GetResponseWithoutCert();

            try
            {
                Org.BouncyCastle.X509.X509Certificate cert = TimestampVerification.GetTsaCert(resp.TimeStampToken);
            }
            catch (TspException e)
            {
                StringAssert.Contains(e.Message, "Timestamp Response does not contain signer certificate");
                return;
            }
            Assert.Fail();
        }

        [Test, Category("Test")]
        public void MatchResponseAndRequest_TimeStampRequestAndTimeStampResponseDontMatch_ShouldThrowTspException()
        {
            byte[] hash;
            using (FileStream fs = new FileStream(cwd + @"\resources\file1.txt", FileMode.Open)) {
                hash = TimestampFile.CreateHash(fs);
            }
            TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();
            reqGen.SetCertReq(true);
            TimeStampRequest req = reqGen.Generate(TspAlgorithms.Sha256, hash, BigInteger.ValueOf(100));
            TimeStampResponse resp = GetResponse();

            Exception e = Assert.Throws<TspException>(() => TimestampVerification.MatchResponseAndRequest(req, resp));
            StringAssert.Contains("Validation of the response failed:", e.Message);
        }

        [Test, Category("Test")]
        public void MatchResponseAndRequest_MatchingResponseAndRequest_ShouldReturnTrue()
        {
            Tuple<TimeStampRequest, TimeStampResponse> ts = GetTuple();

            bool valid = TimestampVerification.MatchResponseAndRequest(ts.Item1, ts.Item2);

            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void VerifyCert_ValidCert_ShouldReturnTrue()
        {
            TimeStampResponse resp = GetResponse();
            Org.BouncyCastle.X509.X509Certificate cert = TimestampVerification.GetTsaCert(resp.TimeStampToken);

            bool valid = TimestampVerification.VerifyCert(cert);

            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void Verify_EverythingCorrect_ShouldReturnTrue()
        {
            Tuple<TimeStampRequest, TimeStampResponse> ts = GetTuple();
            TimeStampRequest req = ts.Item1;
            TimeStampResponse resp = ts.Item2;
            bool valid = false;

            valid = TimestampVerification.Verify(req, resp);
            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void Verify_EverythingCorrectWithCertificateCollection_ShouldReturnTrue()
        {
            Console.WriteLine(TestContext.CurrentContext.WorkDirectory);

            Tuple<TimeStampRequest, TimeStampResponse> ts = GetTuple();
            TimeStampRequest req = ts.Item1;
            TimeStampResponse resp = ts.Item2;
            byte[] certHash = File.ReadAllBytes(cwd + @"\resources\rootcert.crt");
            X509Certificate2 cert = new X509Certificate2(certHash);
            X509Certificate2Collection collection = new X509Certificate2Collection(cert);
            bool valid = false;

            valid = TimestampVerification.Verify(req, resp, collection);
            Assert.IsTrue(valid);
        }
    }
}
