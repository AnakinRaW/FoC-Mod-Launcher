using System;
using System.IO;

namespace FocLauncherApp.Updater
{
    internal static class VersionUtilities
    {
        private static bool _downloadAttempted;

        private static Version _latestLauncherVersionCache;
        private static Version _latestThemeVersionCache;

        public static Version GetLatestVersion(string versionsRelativePath, VersionType versionType)
        {
            if (!_downloadAttempted)
                GetVersionsOnline(versionsRelativePath);
            return GetFromCache(versionType);
        }

        private static void GetVersionsOnline(string versionsRelativePath)
        {
            var server = new UpdateServer(BootstrapperApp.ServerUrl);
            var data = server.DownloadString(versionsRelativePath).ToStream();
            _downloadAttempted = true;
            SerializeVersions(data);
        }

        private static void SerializeVersions(Stream dataStream)
        {
            var reader = new StreamReader(dataStream);
            while (!reader.EndOfStream)
            {
                var entry = reader.ReadLine();
                if (entry == null)
                    continue;
                var items = entry.Split(':');
            }
        }

        private static Version GetFromCache(VersionType versionType)
        {
            switch (versionType)
            {
                case VersionType.Launcher:
                    return _latestLauncherVersionCache;
                case VersionType.Theme:
                    return _latestThemeVersionCache;
            }
            return null;
        }
    }

    internal enum VersionType
    {
        Launcher,
        Theme
    }
}
