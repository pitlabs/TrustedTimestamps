using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timestamping;
using Verify;

namespace TimestampMain
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
    }
}
