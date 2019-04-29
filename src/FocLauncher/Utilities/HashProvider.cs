using System;
using System.IO;
using System.Security.Cryptography;

namespace FocLauncher.Core.Utilities
{
    public class HashProvider
    {
        public string GetFileHash(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            var md5 = new MD5CryptoServiceProvider();
            var fileReader = File.OpenRead(filePath);

            using (fileReader)
            {
                var md5Hash = md5.ComputeHash(fileReader);
                fileReader.Close();
                return Trim(md5Hash);
            }
        }

        private string Trim(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
