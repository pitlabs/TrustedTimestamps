using Org.BouncyCastle.Tsp;
using System;
using System.IO;

namespace Pit.Labs.Timestamp
{
    public class TimestampGenerator
    {
        private bool certReq;
        private bool nonceReq;

        public bool CertReq
        {
            get
            {
                return certReq;
            }
            set
            {
                certReq = value;
            }
        }

        public bool NonceReq
        {
            get
            {
                return nonceReq;
            }
            set
            {
                nonceReq = value;
            }
        }

        public TimestampGenerator()
        {
            certReq = false;
            nonceReq = true;
        }

        public byte[] CreateTimestamp(string hash)
        {
            hash = hash.ToLower();
            byte[] hashByte = new byte[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2)
            {
                hashByte[i / 2] = Convert.ToByte(hash.Substring(i, 2), 16);
            }
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hashByte, certReq, nonceReq);
            TimeStampResponse resp = ts.Item2;
            return resp.GetEncoded();
        }

        public byte[] CreateTimestamp(FileStream fs)
        {
            byte[] hash = TimestampFile.CreateHash(fs);
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hash, certReq, nonceReq);
            TimeStampResponse resp = ts.Item2;
            return resp.GetEncoded();
        }

        public byte[] CreateTimestamp(byte[] hash)
        {
            Tuple<TimeStampRequest, TimeStampResponse> ts = TimestampFile.GetTimestamp(hash, certReq, nonceReq);
            TimeStampResponse resp = ts.Item2;
            return resp.GetEncoded();
        }
    }
}
