using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.FileSystem
{

    public enum ValidationResult
    {
        Success,
        ValidationContextError,
        HashMismatch,
    }

    internal class HashVerifier
    {
        public static ValidationResult Verify(Stream stream, ValidationContext validationContext)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            stream.Seek(0L, SeekOrigin.Begin);
            if (!validationContext.Verify())
                return ValidationResult.ValidationContextError;
            var hash = GetHashOfStream(stream);
            var expected = validationContext.Hash;
            return hash.SequenceEqual(expected) ? ValidationResult.Success : ValidationResult.HashMismatch;
        }

        public static ValidationResult VerifyFile(string file, ValidationContext validationContext)
        {
            if (!validationContext.Verify())
                return ValidationResult.ValidationContextError;
            var hash = GetHashOfFile(file);
            var expected = validationContext.Hash;
            return hash.SequenceEqual(expected) ? ValidationResult.Success : ValidationResult.HashMismatch;
        }

        private static byte[] GetHashOfStream(Stream stream, HashAlgorithmName algorithm = default)
        {
            var name = algorithm.Name ?? "SHA1";
            using var hashAlgorithm = HashAlgorithm.Create(name);
            if (hashAlgorithm == null)
                throw new CryptographicException($"Could not find hashing provider of name: {algorithm}");
            return hashAlgorithm.ComputeHash(stream);
        }

        private static byte[] GetHashOfFile(string path, HashAlgorithmName algorithm = default)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return GetHashOfStream(fileStream, algorithm);
        }
    }
}
