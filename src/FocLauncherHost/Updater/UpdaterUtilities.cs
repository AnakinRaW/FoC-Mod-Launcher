﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater
{
    internal static class UpdaterUtilities
    {
        internal static readonly string UpdaterMutex = $"Global\\{Process.GetCurrentProcess().ProcessName}";

        internal static Version GetAssemblyVersion(string file)
        {
            var assembly = AssemblyName.GetAssemblyName(file);
            return assembly.Version;
        }

        internal static Version GetAssemblyFileVersion(string file)
        {
            var version = FileVersionInfo.GetVersionInfo(file).FileVersion;
            return Version.Parse(version);
        }


        internal static string GetFilePath(this IComponent component)
        {
            if (string.IsNullOrEmpty(component.Name))
                throw new InvalidOperationException();
            if (string.IsNullOrEmpty(component.Destination))
                throw new InvalidOperationException();
            return Path.Combine(component.Destination, component.Name);
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
                case HashType.Sha2:
                    return GetFileHash(file, HashAlgorithmName.SHA256);
                case HashType.Sha3:
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

        // https://stackoverflow.com/a/9995303
        internal static byte[] HexToArray(string input)
        {
            if (input.Length % 2 != 0)
                throw new NotSupportedException("string input lenght is wrong");
            input = input.ToLower();
            var arr = new byte[input.Length >> 1];
            for (var i = 0; i < input.Length >> 1; ++i)
                arr[i] = (byte)((GetHexVal(input[i << 1]) << 4) + (GetHexVal(input[(i << 1) + 1])));
            return arr;
        }

        internal static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                extensions = new[] {".*"};
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private static int GetHexVal(char hex)
        {
            var val = (int)hex;
            return val - (val < 58 ? 48 : 87);
        }

        internal static Mutex CheckAndSetGlobalMutex(string name = null)
        {
            var mutex = EnsureMutex(name);

            if (mutex == null)
                throw new InvalidOperationException("Setup can not run");
            return mutex;
        }

        internal static Mutex? EnsureMutex(string name = null)
        {
            return EnsureMutex(name, TimeSpan.Zero);
        }

        internal static Mutex? EnsureMutex(string name, TimeSpan timeout)
        {
            name ??= UpdaterMutex;
            Mutex mutex;
            try
            {
                mutex = Mutex.OpenExisting(name);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                var mutexSecurity = new MutexSecurity();
                var rule = new MutexAccessRule(securityIdentifier, MutexRights.FullControl, AccessControlType.Allow);
                mutexSecurity.AddAccessRule(rule);
                mutex = new Mutex(false, name, out _, mutexSecurity);
            }

            bool mutexAbandoned;
            try
            {
                mutexAbandoned = mutex.WaitOne(timeout);
            }
            catch (AbandonedMutexException)
            {
                mutexAbandoned = true;
            }
            return mutexAbandoned ? mutex : null;
        }

        public static async Task CopyFileToStreamAsync(string filePath, Stream stream, CancellationToken cancellation = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(stream, 81920, cancellation);
        }
    }
}
