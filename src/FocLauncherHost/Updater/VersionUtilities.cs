using System;
using System.IO;

namespace FocLauncherHost.Updater
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
            var server = new UpdateServer(HostApplication.ServerUrl);
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
                if (items.Length != 2)
                    return;
                var type = (VersionType) Enum.Parse(typeof(VersionType), items[0], true);
                var version = Version.Parse(items[1]);
                SetCache(type, version);
            }
        }

        private static void SetCache(VersionType versionType, Version version)
        {
            switch (versionType)
            {
                case VersionType.Launcher:
                    _latestLauncherVersionCache = version;
                    break;
                case VersionType.Theme:
                    _latestThemeVersionCache = version;
                    break;
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
