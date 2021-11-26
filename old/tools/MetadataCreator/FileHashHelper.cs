using System;
using System.IO;
using System.Security.Cryptography;

namespace MetadataCreator
{
    internal static class FileHashHelper
    {
        public enum HashType
        {
            None,
            MD5,
            Sha1,
            Sha256,
            Sha384,
            Sha512
        }

        internal static byte[] GetFileHash(string file, HashType hashType)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(nameof(file));

            switch (hashType)
            {
                case HashType.MD5:
                    return GetFileHash(file, HashAlgorithmName.MD5);
                case HashType.Sha1:
                    return GetFileHash(file, HashAlgorithmName.SHA1);
                case HashType.Sha256:
                    return GetFileHash(file, HashAlgorithmName.SHA256);
                case HashType.Sha512:
                    return GetFileHash(file, HashAlgorithmName.SHA512);
                default:
                    throw new InvalidOperationException("Unable to find a hashing algorithm");
            }
        }

        private static byte[] GetFileHash(string file, HashAlgorithmName algorithm)
        {
            if (!File.Exists(file))
                return null;
            using var hash = HashAlgorithm.Create(algorithm.Name);
            if (hash == null)
                throw new CryptographicException($"Could not find hashing provider of name: {algorithm}");
            var fileInfo = new FileInfo(file);
            using var fs = fileInfo.OpenRead();
            fs.Position = 0;
            return hash.ComputeHash(fs);
        }
    }
}