using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocLauncherApp.Updater
{
    internal static class VersionUtilities
    {
        public static Version GetLatestFileVersion(string versionsRelativePath)
        {
            var versions = GetAllAvailableThemeVersionsOnline(versionsRelativePath);
            if (versions == null || versions.Count == 0)
                return null;
            return versions.Last();
        }

        private static List<Version> GetAllAvailableThemeVersionsOnline(string versionsRelativePath)
        {
            var server = new UpdateServer(BootstrapperApp.ServerUrl);
            var data = server.DownloadString(versionsRelativePath).ToStream();
            return SerializeVersionsToList(data);
        }

        private static List<Version> SerializeVersionsToList(Stream dataStream)
        {
            var list = new List<Version>();
            var reader = new StreamReader(dataStream);
            while (!reader.EndOfStream)
                list.Add(new Version(reader.ReadLine()));
            return list;
        }
    }
}
