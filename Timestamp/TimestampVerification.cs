using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Cms;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Verify
{
    public class TimestampVerification
    {

        public static Org.BouncyCastle.X509.X509Certificate GetTsaCert(TimeStampToken token)
        {
            var signerCertificates = token.GetCertificates("Collection").GetMatches(token.SignerID);
            // Check if the response contains a signer certificate.
            if (signerCertificates.Count == 0)
            {
                Console.WriteLine("ERROR: The Timestamp Response does not contain a signer certificate. Try entering -cert when you create a Timestamp.");
                throw new TspException("Timestamp Response does not contain signer certificate");
            }
            else
            {
                // Extract the signer certificate from the response.
                Org.BouncyCastle.X509.X509Certificate cert = null;
                foreach (var match in signerCertificates)
                {
                    cert = (Org.BouncyCastle.X509.X509Certificate)match;
                    break;
                }
                if (cert == null)
                {
                    Console.WriteLine("ERROR: There must be a certificate in the Timestamp Response.");
                    throw new TspException("Must be certificate in TimestampResponse");
                }
                return cert;
            }
        }

        public static bool MatchResponseAndRequest(TimeStampRequest req, TimeStampResponse resp)
        {
            bool valid = false;
            try
            {
                resp.Validate(req);
                valid = true;
            }
            catch (TspException e)
            {
                Console.WriteLine("ERROR: Timestamp Response and Timestamp Request do not match.");
                throw new TspException("Validation of the response failed: " + e.Message);
            }
            return valid;
        }

        public static bool ValidateResponse(TimeStampToken tsToken, Org.BouncyCastle.X509.X509Certificate cert)
        {
            bool valid = false;
            try
            {
                tsToken.Validate(cert);
                valid = true;
            }
            catch (TspException e)
            {
                Console.WriteLine("ERROR: Timestamp Response could not be validated using the Signer Certificate.");
                throw new TspException("Could not validate response using signer certificate: " + e.Message);
            }
            return valid;
        }

        public static bool VerifyCert(Org.BouncyCastle.X509.X509Certificate cert)
        {
            byte[] tsaCertByte = cert.GetEncoded();
            X509Certificate2 signerCert = new X509Certificate2(tsaCertByte);
            bool validation = signerCert.Verify();
            if (!validation)
            {
                Console.WriteLine("ERROR: The Signer Certificate could not be verified. Check your installed certificates.");
                throw new TspException("Signer Certificate could not be verified.");
            }
            return validation;
        }

        public static bool Verify(TimeStampRequest request, TimeStampResponse response)
        {
            // Check if the given response and request match.
            bool match = MatchResponseAndRequest(request, response);

            // Get the signer certificate.
            Org.BouncyCastle.X509.X509Certificate tsaCert = null;
            TimeStampToken tsToken = response.TimeStampToken;
            tsaCert = GetTsaCert(tsToken);
            // Try to validate the response using the signer certificate.
            bool validToken = ValidateResponse(tsToken, tsaCert);

            // Validate the signer certificate itself.
            bool validCert = VerifyCert(tsaCert);
            return ((match && validToken) && validCert);
        }

        public static bool Verify(TimeStampRequest request, TimeStampResponse response, X509Certificate2Collection certCollection)
        {
            // Save all given Certificates into a Certificate Store before Verification.
            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadWrite);
            X509Certificate2Collection storeCollection = store.Certificates;
            foreach(X509Certificate2 cert in certCollection)
            {
                if(!storeCollection.Contains(cert))
                {
                    store.Add(cert);
                }
            }
            store.Close();
            return Verify(request, response);
        }
    }
}
