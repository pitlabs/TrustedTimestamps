using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using System;
using System.IO;

namespace Pit.Labs.Timestamp
{
    public class TimestampVerifyer
    {
        public bool GetVerification(byte[] timestamp, string originalHash)
        {
            TimeStampResponse resp = new TimeStampResponse(timestamp);
            originalHash = originalHash.ToLower();
            byte[] hashByte = new byte[originalHash.Length / 2];
            for (int i = 0; i < originalHash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(originalHash.Substring(i, 2), 16);
            }
            BigInteger nonce = resp.TimeStampToken.TimeStampInfo.Nonce;
            TimeStampRequest req = TimestampFile.CreateTimestampRequest(hashByte, true, nonce);
            return TimestampVerification.Verify(req, resp);
        }

        public bool GetVerification(byte[] timestamp, FileStream originalFs)
        {
            TimeStampResponse resp = new TimeStampResponse(timestamp);
            BigInteger nonce = resp.TimeStampToken.TimeStampInfo.Nonce;
            byte[] hash = TimestampFile.CreateHash(originalFs);
            TimeStampRequest req = TimestampFile.CreateTimestampRequest(hash, true, nonce);
            return TimestampVerification.Verify(req, resp);
        }

        public bool GetVerification(byte[] timestamp, byte[] originalHash)
        {
            TimeStampResponse resp = new TimeStampResponse(timestamp);
            BigInteger nonce = resp.TimeStampToken.TimeStampInfo.Nonce;
            TimeStampRequest req = TimestampFile.CreateTimestampRequest(originalHash, true, nonce);
            return TimestampVerification.Verify(req, resp);
        }
    }
}
