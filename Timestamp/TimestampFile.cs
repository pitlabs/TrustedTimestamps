using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verify;

namespace Timestamping
{
    public class TimestampFile
    {

        public static byte[] CreateHash(Stream stream)
        {
            // Compute the Hash of the file at the given path.
            HashAlgorithm sha = new SHA256Managed();
            try
            {
                sha.ComputeHash(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Hash couldn't be computed.");
                throw new TspException("Hash could not be computed: " + e.Message);
            }
            return sha.Hash;
        }

        public static TimeStampRequest CreateTimestampRequest(byte[] digest, bool certReq, BigInteger nonce)
        {
            // Generate the TimeStampRequest.
            TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();
            reqGen.SetCertReq(certReq);
            TimeStampRequest request;
            if (nonce != null)
            {
                request = reqGen.Generate(TspAlgorithms.Sha256, digest, nonce);
            }
            else
            {
                request = reqGen.Generate(TspAlgorithms.Sha256, digest);
            }
            return request;
        }

        public static WebRequest CreateWebRequest(byte[] content, string site, string method, string contentType)
        {
            // Create a new WebRequest and post the request to the TSA.
            WebRequest webReq = WebRequest.Create(site);
            webReq.Method = method;
            webReq.ContentType = contentType;
            webReq.ContentLength = content.Length;
            return webReq;
        }

        public static WebResponse PostTimestampRequest(TimeStampRequest req, HttpWebRequest webReq)
        {
            byte[] reqData = req.GetEncoded();
            using (Stream requestStream = webReq.GetRequestStream())
            {
                try
                {
                    requestStream.Write(reqData, 0, reqData.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Error in communication with the DFN PKI.");
                    throw new TspException("Error in communicating with DFN PKI: " + e.Message);
                }
            }
            // Receive the WebResponse.
            WebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            return webResp;
        }

        public static TimeStampResponse CreateTimestampResponse(HttpWebResponse webResp)
        {
            // Create a Timestamp Response from the given Web Response.
            TimeStampResponse resp;
            using (Stream responseStream = new BufferedStream(webResp.GetResponseStream()))
            {
                resp = new TimeStampResponse(responseStream);
            }
            return resp;
        }

        public static void WriteTsResponseToStream(TimeStampResponse resp, Stream stream)
        {
            // Save the Response at the given path.
            try
            {
                stream.Write(resp.GetEncoded(), 0, resp.GetEncoded().Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Could not write response to given path.");
                throw new TspException("Writing response to path not possible: " + e.Message);
            }
        }

        public static Tuple<TimeStampRequest, TimeStampResponse> GetTimestamp(string pathIn, string pathOut, bool certReq, bool nonce)
        {
            // Create hash of input file.
            byte[] hash;
            using (FileStream fsStream = new FileStream(pathIn, FileMode.Open))
            {
                hash = TimestampFile.CreateHash(fsStream);
            }
            // Create Timestamp Request.
            return GetTimestamp(hash, pathOut, certReq, nonce);
        }

        public static BigInteger GetNonce(bool nonceReq)
        {
            if (nonceReq)
            {
                Random rnd = new Random();
                return new BigInteger(64, rnd);
            }
            else
            {
                return null;
            }
        }

        public static Tuple<TimeStampRequest, TimeStampResponse> GetTimestamp(byte[] hash, string pathOut, bool certReq, bool nonceReq)
        {
            // Create Timestamp Request.
            TimeStampRequest req;
            BigInteger nonce = GetNonce(nonceReq);
            req = CreateTimestampRequest(hash, certReq, nonce);
            // Create Timestamp Response.
            HttpWebRequest webReq = (HttpWebRequest)CreateWebRequest(req.GetEncoded(), "http://zeitstempel.dfn.de/", "POST", "application/timestamp-query");
            HttpWebResponse webResp = (HttpWebResponse)PostTimestampRequest(req, webReq);
            TimeStampResponse resp = TimestampFile.CreateTimestampResponse(webResp);
            // Write the Response to the given path.
            using (FileStream fsOut = new FileStream(pathOut, FileMode.Create))
            {
                WriteTsResponseToStream(resp, fsOut);
            }
            // Return the result.
            Tuple<TimeStampRequest, TimeStampResponse> result = new Tuple<TimeStampRequest, TimeStampResponse>(req, resp);
            return result;
        }
    }
}
