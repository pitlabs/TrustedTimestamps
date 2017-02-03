using Org.BouncyCastle.Tsp;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Math;
using Pit.Labs.Timestamp;

namespace Pit.Labs.TimestampConsole
{
    public class Timestamp
    {
        public static string[] SortArguments(string[] arguments)
        {
            // Program only runs if there are valid arguments in the string[] args.
            if (arguments.Length == 0)
            {
                Console.WriteLine("ERROR: Please enter arguments");
                throw new ArgumentOutOfRangeException("No arguments were entered");
            }
            else
            {
                string[] sorted = new string[7] { "", "", "", "", "", "", "" };
                // Since only timestamping is possible, only ts is allowed.
                if (arguments[0].Equals("ts"))
                {
                    // Check if a new timestamp should be created.
                    if (arguments[1].Equals("-query"))
                    {
                        sorted[0] = arguments[1];
                        // Sort all given commands.
                        for (int i = 2; i < arguments.Length; i++)
                        {
                            // Check for -data or -digest command and corresponding path.
                            if ((arguments[i].Equals("-data") || arguments[i].Equals("-digest")) && sorted[1].Length == 0)
                            {
                                sorted[1] = arguments[i];
                                try
                                {
                                    sorted[2] = arguments[i + 1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR: {0} was entered but no matching path or digest was found.", arguments[i]);
                                    throw new ArgumentOutOfRangeException("Error getting data: " + e.Message);
                                }
                                i++;
                            }
                            // Check for -no_nonce command.
                            else if (arguments[i].Equals("-no_nonce") && sorted[3].Length == 0)
                            {
                                sorted[3] = arguments[i];
                            }
                            // Check for -cert command.
                            else if (arguments[i].Equals("-cert") && sorted[4].Length == 0)
                            {
                                sorted[4] = arguments[i];
                            }
                            // Check for -out command and corresponding path.
                            else if (arguments[i].Equals("-out") && sorted[5].Length == 0)
                            {
                                sorted[5] = arguments[i];
                                try
                                {
                                    sorted[6] = arguments[i + 1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR: {0} was entered but no matching path was found.", arguments[i]);
                                    throw new ArgumentOutOfRangeException("Error getting data: " + e.Message);
                                }
                                i++;
                            }
                            // Check for any invalid commands and any valid commands given twice.
                            else
                            {
                                Console.WriteLine("ERROR: {0} is an invalid argument in combination with -query.", arguments[i]);
                                throw new ArgumentOutOfRangeException("Invalid argument was entered.");
                            }
                        }
                        // Check that all necessary commands were given.
                        if (sorted[1].Length == 0 || sorted[2].Length == 0 || sorted[5].Length == 0 || sorted[6].Length == 0)
                        {
                            Console.WriteLine("ERROR: '-data filepath' or '-digest hash' and '-out filepath' must be entered in combination with -query!");
                            throw new ArgumentOutOfRangeException("Argument was not entered.");
                        }
                        else
                        {
                            return sorted;
                        }
                    }
                    // Check if a timestamp should be verified.
                    else if (arguments[1].Equals("-verify"))
                    {
                        sorted[0] = arguments[1];
                        // Sort all given arguments.
                        for (int i = 2; i < arguments.Length; i++)
                        {
                            // Check for -data, -digest or -queryfile command and the corresponding path or digest.
                            if ((arguments[i].Equals("-data") || arguments[i].Equals("-digest") || arguments[i].Equals("-queryfile")) && sorted[1].Length == 0)
                            {
                                sorted[1] = arguments[i];
                                try
                                {
                                    sorted[2] = arguments[i + 1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR: {0} was entered but no matching path or digest was found.", arguments[i]);
                                    throw new ArgumentOutOfRangeException("Error getting data: " + e.Message);
                                }
                                i++;
                            }
                            // Check for -in command and the corresponding path.
                            else if (arguments[i].Equals("-in") && sorted[3].Length == 0)
                            {
                                sorted[3] = arguments[i];
                                try
                                {
                                    sorted[4] = arguments[i + 1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR: {0} was entered but no matching path was found.", arguments[i]);
                                    throw new ArgumentOutOfRangeException("Error getting data: " + e.Message);
                                }
                                i++;
                            }
                            // Check for -CApath or -CAfile command and the corresponding path.
                            else if ((arguments[i].Equals("-CApath") || arguments[i].Equals("-CAfile")) && sorted[5].Length == 0)
                            {
                                sorted[5] = arguments[i];
                                try
                                {
                                    sorted[6] = arguments[i + 1];
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("ERROR: {0} was entered but no matching path was found.", arguments[i]);
                                    throw new ArgumentOutOfRangeException("Error getting data: " + e.Message);
                                }
                                i++;
                            }
                            // Check for any invalid arguments or a valid argument entered twice.
                            else
                            {
                                Console.WriteLine("ERROR: {0} is an invalid argument in combination with -verify.", arguments[i]);
                                throw new ArgumentOutOfRangeException("Invalid argument entered");
                            }
                        }
                        // Check that all necessary commands were given.
                        if (sorted[1].Length == 0 || sorted[2].Length == 0 || sorted[3].Length == 0 || sorted[4].Length == 0)
                        {
                            Console.WriteLine("ERROR: '-data filepath' or '-digest hash' or 'queryfile request.tsq' and '-in filepath' are necessary");
                            throw new ArgumentOutOfRangeException("Argument wasn't entered");
                        }
                        else
                        {
                            return sorted;
                        }
                    }
                    // Check for any invalid second command.
                    else
                    {
                        Console.WriteLine("ERROR: Invalid argument: " + arguments[1] + ". Only -query or -verify are valid arguments here.");
                        throw new ArgumentOutOfRangeException("Invalid arguments were entered. Only Query or Verify possible.");
                    }
                }
                // Display help: list of all possible commands.
                else if (arguments[0].Equals("-help"))
                {
                    sorted[0] = "-help";
                    Console.WriteLine("-ts Timestamp Creation or Verification");
                    Console.WriteLine("  -query Create a new Timestamp");
                    Console.WriteLine("    -data File to be timestamped (this or digest is necessary)");
                    Console.WriteLine("    -digest Hash to be timestamped (this or data is necessary)");
                    Console.WriteLine("    -out Path where the Response is saved");
                    Console.WriteLine("    -no_nonce The Timestamp is created without a nonce (optional)");
                    Console.WriteLine("    -cert The Response contains a signer certificate (optional)");
                    Console.WriteLine("  -verify Verify an existing timestamp");
                    Console.WriteLine("    -data File that was timestamped (this, digest or queryfile is necessary");
                    Console.WriteLine("    -digest Hash that was timestamped (this, data or queryfile is necessary");
                    Console.WriteLine("    -queryfile Timestamp Request (this, data or digest is necessary");
                    Console.WriteLine("    -in Timestamp Response to be verified");
                    Console.WriteLine("    -CApath Path of certificate to be used in the verification (optional)");
                    Console.WriteLine("    -CAfile Path of certificate chain to be used in the verification (optional)");
                    Console.WriteLine("Possible return codes:");
                    Console.WriteLine(" 0: Program ran normally");
                    Console.WriteLine(" 1: There was an error in the given arguments");
                    Console.WriteLine(" 2: There was an error during Timestamping/Verification");
                    Console.WriteLine(" 10: There was an unexpected, unknown error");
                    return sorted;
                }
                // Check for any invalid first commands.
                else
                {
                    Console.WriteLine("ERROR: Invalid argument: " + arguments[0] + ". Only -ts or -help is a valid argument here.");
                    throw new ArgumentOutOfRangeException("Invalid arguments were entered. Only ts possible.");
                }
            }
        }

        public static TimeStampResponse GetResponse(string path)
        {
            byte[] responseBytes;
            // Read the Timestamp Response from the file.
            try
            {
                responseBytes = File.ReadAllBytes(path);
            }
            catch
            {
                Console.WriteLine("ERROR: Response File could not be read.");
                throw new ArgumentOutOfRangeException("Invalid response file path given");
            }
            // Create and return the Timestamp Response.
            TimeStampResponse response = new TimeStampResponse(responseBytes);
            return response;
        }

        public static X509Certificate2Collection GetCertificates(string[] arguments)
        {
            string path = arguments[1];
            // If single certificate is given, copy the certificate from the file and return a collection containing only that certificate.
            if (arguments[0].Equals("-CApath"))
            {
                byte[] certBytes;
                // Try to read the certificate from the file.
                try
                {
                    certBytes = File.ReadAllBytes(path);
                }
                catch
                {
                    Console.WriteLine("ERROR: Certificate at certificate path could not be read.");
                    throw new ArgumentOutOfRangeException("Certificate path invalid");
                }
                // Create and return a Certificate Collection.
                X509Certificate2 cert = new X509Certificate2(certBytes);
                return new X509Certificate2Collection(cert);
            }
            else if (arguments[0].Equals("-CAfile"))
            {
                // Read the entire chain into a string.
                string chain = File.ReadAllText(path);
                string certEnd = "-----END CERTIFICATE-----";
                int index = 0;
                X509Certificate2Collection collection = new X509Certificate2Collection();
                do
                {
                    // Save the first certificate in a string of its own.
                    chain = chain.Substring(chain.IndexOf("subject"));
                    index = chain.IndexOf(certEnd) + certEnd.Length;
                    string cert = chain.Substring(0, index);
                    // Create a new Certificate from the string.
                    X509Certificate2 certificate = new X509Certificate2(Encoding.UTF8.GetBytes(cert));
                    // Add the Certificate to the collection.
                    collection.Add(certificate);
                    // Delete the first certificate from the chain string.
                    chain = chain.Substring(index);
                } while (chain.Contains("subject"));
                return collection;
            }
            else
            {
                Console.WriteLine("ERROR: No -CApath or -CAfile found in arguments");
                throw new ArgumentOutOfRangeException("No certificates in arguments");
            }
        }

        public static Tuple<TimeStampRequest, TimeStampResponse> Query(string[] arguments)
        {
            // Get all the necessary data from the sorted Array.
            bool cert = arguments[4].Equals("-cert");
            bool nonceReq = arguments[3].Length == 0;
            string pathOut = arguments[6];
            Tuple<TimeStampRequest, TimeStampResponse> result;
            string data = arguments[2];
            // Create the timestamp using a given file.
            if (arguments[1].Equals("-data"))
            {
                result = TimestampFile.GetTimestamp(data, pathOut, cert, nonceReq);
            }
            // Create the timestamp using a given.
            else
            {
                // Convert the string to a byte array.
                data = data.ToLower();
                byte[] hash = new byte[data.Length / 2];
                for (int i = 0; i < data.Length; i += 2)
                {
                    hash[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
                }
                // Create the timestamp.
                result = TimestampFile.GetTimestamp(hash, pathOut, cert, nonceReq);
            }
            return result;
        }

        public static bool Verify(string[] arguments)
        {
            // Get all the necessary data from the args Array.
            string data = arguments[2];
            TimeStampResponse resp = GetResponse(arguments[4]);
            // Create a new Timestamp Request to be used in the verification.
            TimeStampRequest req;
            // Check if a request file was given.
            if (arguments[1].Equals("-queryfile"))
            {
                // Read the request from the file.
                byte[] reqBytes = File.ReadAllBytes(data);
                req = new TimeStampRequest(reqBytes);
            }
            // Check if a digest was given.
            else if (arguments[1].Equals("-digest"))
            {
                // Convert the string to a byte array.
                data = data.ToLower();
                byte[] hash = new byte[data.Length / 2];
                for (int i = 0; i < data.Length; i += 2)
                {
                    hash[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
                }
                // Set the nonce to be equal to the responses nonce.
                BigInteger nonce = resp.TimeStampToken.TimeStampInfo.Nonce;
                // Create a new Timestamp request using the byte array and the nonce.
                req = TimestampFile.CreateTimestampRequest(hash, true, nonce);
            }
            // The original file was given.
            else
            {
                // Create a hash of the file.
                byte[] hash;
                using (FileStream fs = new FileStream(data, FileMode.Open))
                {
                    hash = TimestampFile.CreateHash(fs);
                }
                // Set the nonce to be equal to the responses nonce.
                BigInteger nonce = resp.TimeStampToken.TimeStampInfo.Nonce;
                // Create a new Timestamp Request using the byte array and the nonce.
                req = TimestampFile.CreateTimestampRequest(hash, true, nonce);
            }
            // Verify the response.
            bool verified = false;
            // Check if a certificate or a chain should be used for the Verification.
            if (!(arguments[5].Length == 0))
            {
                X509Certificate2Collection collection = GetCertificates(new string[] { arguments[5], arguments[6]});
                verified = TimestampVerification.Verify(req, resp, collection);
            }
            // Verification without additional certificates.
            else
            {
                verified = TimestampVerification.Verify(req, resp);
            }
            return verified;
        }

        static int Main(string[] args)
        {
            try
            {
                string[] sorted = SortArguments(args);
                // Create a new Timestamp.
                if (sorted[0].Equals("-query"))
                {
                    Tuple<TimeStampRequest, TimeStampResponse> result;
                    result = Query(sorted);
                    // If the response contains a signer certificate, check whether it is correct.
                    bool verified = false;
                    Console.WriteLine("Timestamp was created successfully.");
                    if (sorted[4].Equals("-cert"))
                    {
                        TimeStampRequest req = result.Item1;
                        TimeStampResponse resp = result.Item2;
                        try
                        {
                            verified = TimestampVerification.Verify(req, resp);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR: The created timestamp could not be verified. Please try again.");
                            Console.WriteLine(e.Message);
                        }
                    }
                    if (verified)
                    {
                        Console.WriteLine("Timestamp was verified successfully.");
                    }
                }
                // Verify an existing timestamp.
                else if (sorted[0].Equals("-verify"))
                {
                    bool verified = false;
                    verified = Verify(sorted);
                    if (verified)
                    {
                        Console.WriteLine("Timestamp was verified successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Timestamp could not be verified successfully.");
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return 1;
            }
            catch (TspException)
            {
                return 2;
            }
            catch
            {
                return 10;
            }
            return 0;
        }
    }
}
