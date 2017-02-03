using System;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Tsp;
using System.IO;
using NUnit.Framework;
using Pit.Labs.TimestampConsole;

namespace TimestampTest
{
    [TestFixture]
    public class TimestampTesting
    {
        readonly string cwd = TestContext.CurrentContext.WorkDirectory;

        [Test, Category("Test")]
        public void SortArguments_NotsCommandAtBeginningOfArray_ShouldThrowArgumentOutOfRange()
        {
            string[] array = { "-query", "-data", cwd + @"\resources\file1.txt", "-out", cwd + @"\resources\response.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid arguments were entered. Only ts possible.", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_NoQueryOrVerifyAtBeginningOfArray_ShouldThrowArgumentOutOfRange()
        {
            string[] array = { "ts", "-data", cwd + @"\resources\file1.txt", "-out", cwd + @"\resources\response.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid arguments were entered. Only Query or Verify possible.", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_EmptyArray_ShouldThrowArgumentOutOfRange()
        {
            string[] array = new String[0];

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("No arguments were entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithInvalidArgumentQuery_ShouldThrowArgumentOutOfRange()
        {
            //Query
            string[] array = { "ts", "-query", "-queryfile", cwd + @"\resources\file1.txt", "-out", cwd + @"\resources\response2.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid argument was entered.", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithInvalidArgumentVerify_ShouldThrowArgumentOutOfRange() {
            //Verify
            string[] array = { "ts", "-verify", "-no_nonce", cwd + @"\resources\file1.txt", "-out", cwd + @"\resources\response2.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid argument entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithDoubleCommandQuery_ShouldThrowArgumentOutOfRange()
        {
            //Query with double command
            string[] array = { "ts", "-query", "-out", cwd + @"\resources\file1.txt", "-out", cwd + @"\resources\response2.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid argument was entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithDoubleCommandVerify_ShouldThrowArgumentOutOfRange()
        {
            //Verify with double command
            string[] array = { "ts", "-verify", "-in", cwd + @"\resources\file1.txt", "-in", cwd + @"\resources\response2.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Invalid argument entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithQueryAndCert_ShouldReturnCorrectSortedArray()
        {
            string[] array = { "ts", "-query", "-out", cwd + @"\resources\response2.tsr", "-data", cwd + @"\resources\file1.txt", "-cert" };
            string[] sorted = { "-query", "-data", cwd + @"\resources\file1.txt", "", "-cert", "-out", cwd + @"\resources\response2.tsr" };

            string[] actual = Timestamp.SortArguments(array);
            CollectionAssert.AreEqual(actual, sorted);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithVerifyWithoutInPath_ShouldThrowArgumentOutOfRange()
        {
            string[] array = { "ts", "-verify", "-data", cwd + @"\resources\response2.tsr", "-in" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Error getting data: ", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithVerifyAndCAfileWithoutPath_ShouldThrowArgumentOutOfRange()
        {
            string[] array = { "ts", "-verify", "-data", cwd + @"\resources\response2.tsr", "-in", cwd + @"\resources\file1.txt", "-CAfile" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Error getting data: ", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_ArrayWithVerifyAndCAfile_ShouldReturnCorrectArraySorted()
        {
            string[] array = { "ts", "-verify", "-data", cwd + @"\resources\response2.tsr", "-in", cwd + @"\resources\file1.txt", "-CAfile", cwd + @"\resources\chain.pem" };
            string[] sorted = { "-verify", "-data", cwd + @"\resources\response2.tsr", "-in", cwd + @"\resources\file1.txt", "-CAfile", cwd + @"\resources\chain.pem" };

            string[] actual = Timestamp.SortArguments(array);
            CollectionAssert.AreEqual(actual, sorted);
        }

        [Test, Category("Test")]
        public void SortArguments_DataWithoutPathsQuery_ShouldThrowArgumentOutOfRange()
        {
            //Query
            string[] array = { "ts", "-query", "-out", cwd + @"\resources\response.tsr", "-data" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Error getting data: ", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_DataWithoutPathsVerify_ShouldThrowArgumentOutOfRange()
        {
            //Verify
            string[] array = { "ts", "-verify", "-in", cwd + @"\resources\response.tsr", "-data" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Error getting data: ", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_QueryOutWithoutPath_ShouldThrowArgumentOutOfRange()
        {
            string[] array = { "ts", "-query", "-data", cwd + @"\resources\response.tsr", "-out" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Error getting data: ", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_NecessaryArgumentMissingQueryNoDataOrDigest_ShouldThrowArgumentOutOfRange()
        {
            //Query: No data/digest; No out
            string[] array = { "ts", "-query", "-out", cwd + @"\resources\response.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Argument was not entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_NecessaryArgumentsMissingVerifyNoDataDigestOrQueryfile_ShouldThrowArgumentOutOfRangeException() {
            //Verify: No data/digest/queryfile; No in
            string[] array = { "ts", "-verify", "-in", cwd + @"\resources\response.tsr" };

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.SortArguments(array));
            StringAssert.Contains("Argument wasn't entered", e.Message);
        }

        [Test, Category("Test")]
        public void SortArguments_HelpCommand_ShouldReturnArrayWithHelpAsFirstCommand()
        {
            string[] array = { "-help" };
            string[] expected = { "-help", "", "", "", "", "", "" };

            string[] actual = Timestamp.SortArguments(array);
            CollectionAssert.AreEqual(actual, expected);
        }

        [Test, Category("Test")]
        public void SortArguments_CorrectQueryArray_ShouldReturnSortedArrays()
        {
            //Query
            string[] array = { "ts", "-query", "-out", cwd + @"\resources\response2.tsr", "-data", cwd + @"\resources\file1.txt", "-no_nonce" };
            string[] sortedExpected = { "-query", "-data", cwd + @"\resources\file1.txt", "-no_nonce", "", "-out", cwd + @"\resources\response2.tsr" };

            string[] sortedActual = Timestamp.SortArguments(array);

            CollectionAssert.AreEqual(sortedExpected, sortedActual);
        }

        [Test, Category("Test")]
        public void SortArguments_CorrectVerifyArray_ShouldReturnSortedArrays() { 
            //Verify
            string[] array = { "ts", "-verify", "-in", cwd + @"\resources\response2.tsr", "-data", cwd + @"\resources\file1.txt" };
            string[] sortedExpected = { "-verify", "-data", cwd + @"\resources\file1.txt", "-in", cwd + @"\resources\response2.tsr", "", "" };

            string[] sortedActual = Timestamp.SortArguments(array);

            CollectionAssert.AreEqual(sortedExpected, sortedActual);
        }

        [Test, Category("Test")]
        public void GetResponse_InvalidPath_ShouldThrowArgumentOutOfRange()
        {
            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.GetResponse(cwd + @"\resources\responseInvalid.tsr"));
            StringAssert.Contains("Invalid response file path given", e.Message);
        }

        [Test, Category("Test")]
        public void GetResponse_ValidPath_ShouldReturnTimeStampResponse()
        {
            TimeStampResponse resp = Timestamp.GetResponse(cwd + @"\resources\response2.tsr");
            byte[] expected = File.ReadAllBytes(cwd + @"\resources\response2.tsr");

            byte[] actual = resp.GetEncoded();
            CollectionAssert.AreEqual(actual, expected);
        }

        [Test, Category("Test")]
        public void GetCertificates_ArrayWithoutCApathOrCAfile_ShouldThrowArgumentOutOfRangeException()
        {
            string[] sorted = { "-verify", "-data", cwd + @"\resources\file1.txt", "-in", cwd + @"\resources\response2.tsr", "", "" };
            string msg = "No certificates in arguments";

            Exception e = Assert.Throws<ArgumentOutOfRangeException>(() => Timestamp.GetCertificates(sorted));
            StringAssert.Contains(msg, e.Message);
        }

        [Test, Category("Test")]
        public void GetCertificates_CAfileWithValidPath_ShouldReturnCollectionWithValidCertificate()
        {
            string[] sorted = {"-CAfile", cwd + @"\resources\chain.pem" };

            X509Certificate2Collection collection = Timestamp.GetCertificates(sorted);
            bool isEmpty = collection.Count == 0;
            Assert.IsFalse(isEmpty);
        }

        [Test, Category("Test")]
        public void GetCertificates_CApathWithValidPath_ShouldReturnCollectionWith3ValidCertificates()
        {
            string[] sorted = {"-CApath", cwd + @"\resources\rootcert.crt" };

            X509Certificate2Collection collection = Timestamp.GetCertificates(sorted);
            bool isEmpty = collection.Count == 0;
            Assert.IsFalse(isEmpty);
        }

        [Test, Category("Test")]
        public void Query_ValidSortedStringArray_ShouldReturnTupleWithValidResponseAndRequestAndSaveResponseCorrectlyToFile()
        {
            string[] sorted = { "-query", "-data", cwd + @"\resources\file1.txt", "-no_nonce", "-cert", "-out", cwd + @"\resources\responsex.tsr" };

            Tuple<TimeStampRequest, TimeStampResponse> result = Timestamp.Query(sorted);
            bool sameResponse = false;
            bool validResponse = false;
            TimeStampRequest req = result.Item1;
            TimeStampResponse resp1 = result.Item2;
            byte[] respHash = File.ReadAllBytes(cwd + @"\resources\responsex.tsr");
            TimeStampResponse resp2 = new TimeStampResponse(respHash);

            try
            {
                CollectionAssert.AreEqual(resp1.GetEncoded(), resp2.GetEncoded());
                sameResponse = true;
            }
            catch { }
            resp1.Validate(req);
            validResponse = true;

            Assert.IsTrue(sameResponse && validResponse);
        }

        [Test, Category("Test")]
        public void Query_ValidSortedStringArrayWithDigest_ShouldReturnValidResponseAndRequest()
        {
            string[] sorted = { "-query", "-digest", "625C6CEAE95C76B1A2ED72DD97DF7EC38139B0DC136A00F6CE537F76F5B21093", "-no_nonce", "-cert", "-out", cwd + @"\resources\responsex2.tsr" };

            Tuple<TimeStampRequest, TimeStampResponse> result = Timestamp.Query(sorted);
            bool sameResponse = false;
            bool validResponse = false;
            TimeStampRequest req = result.Item1;
            TimeStampResponse resp1 = result.Item2;
            byte[] respHash = File.ReadAllBytes(cwd + @"\resources\responsex2.tsr");
            TimeStampResponse resp2 = new TimeStampResponse(respHash);

            try
            {
                CollectionAssert.AreEqual(resp1.GetEncoded(), resp2.GetEncoded());
                sameResponse = true;
            }
            catch { }
            resp1.Validate(req);
            validResponse = true;

            Assert.IsTrue(sameResponse && validResponse);
        }

        [Test, Category("Test")]
        public void Verify_ValidSortedStringArrayWithRequestAndResponseFile_ShouldReturnTrue()
        {
            string[] sorted = { "-verify", "-queryfile", cwd + @"\resources\request2.tsq", "-in", cwd + @"\resources\response2.tsr", "", "" };

            bool valid = Timestamp.Verify(sorted);
            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void Verify_ValidSortedStringArrayWithDigestAndResponseFile_ShouldReturnTrue()
        {
            String[] sorted = { "-verify", "-digest", "625C6CEAE95C76B1A2ED72DD97DF7EC38139B0DC136A00F6CE537F76F5B21093", "-in", cwd + @"\resources\responsex2.tsr", "", "" };

            bool valid = Timestamp.Verify(sorted);
            Assert.IsTrue(valid);
        }

        [Test, Category("Test")]
        public void Verify_ValidSortedStringArrayWithDataAndResponseFile_ShouldReturnTrue()
        {
            String[] sorted = { "-verify", "-data", cwd + @"\resources\file1.txt", "-in", cwd + @"\resources\responsex.tsr", "", "" };

            bool valid = Timestamp.Verify(sorted);
            Assert.IsTrue(valid);
        }
    }
}
