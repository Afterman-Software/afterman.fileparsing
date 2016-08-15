using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AI.Common.Security
{
    public class Hasher
    {
        public static byte[] GetMd5HashBytesForString(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetMd5HashStringForString(string inputString)
        {
            byte[] hash = GetMd5HashBytesForString(inputString);

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static byte[] GetMd5HashBytesForFile(string filePath)
        {
            using (var algorithm = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return algorithm.ComputeHash(stream);
                }
            }
        }

        public static string GetMd5HashStringForFile(string filePath)
        {
            byte[] hash = GetMd5HashBytesForFile(filePath);

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string GetMd5HashCodeStringForFileContents(string filePath)
        {
            int hashCode = GetHashCodeForFileContents(filePath);

            byte[] hash = GetMd5HashBytesForString(hashCode.ToString());

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static byte[] GetSha1HashBytesForString(string inputString)
        {
            HashAlgorithm algorithm = SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetSha1HashStringForString(string inputString)
        {
            byte[] hash = GetSha1HashBytesForString(inputString);

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static byte[] GetSha1HashBytesForFile(string filePath)
        {
            using (var algorithm = SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return algorithm.ComputeHash(stream);
                }
            }
        }

        public static string GetSha1HashStringForFile(string filePath)
        {
            byte[] hash = GetSha1HashBytesForFile(filePath);

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string GetSha1HashCodeStringForFileContents(string filePath)
        {
            int hashCode = GetHashCodeForFileContents(filePath);

            byte[] hash = GetSha1HashBytesForString(hashCode.ToString());

            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }


        public static int GetHashCodeForFileContents(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using (var sr = new StreamReader(stream))
                {
                    int hash = 17;
                    while (!sr.EndOfStream)
                    {
                        string currentString = sr.ReadLine();
                        unchecked
                        {
                            hash = hash * 23 + currentString.GetHashCode();
                        }
                    }
                    return hash;
                }
            }
        }
    }
}