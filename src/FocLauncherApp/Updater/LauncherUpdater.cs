using System;
using System.Diagnostics;
using System.IO;
using FocLauncherApp.Properties;

namespace FocLauncherApp.Updater
{
    internal class LauncherUpdater : AssemblyUpdater
    {
        public override string FilePath => Path.Combine(BootstrapperApp.AppDataPath, "FocLauncher.Core.dll");
        public override string VersionsServerPath => "master/Releases/AvailableUpdates.txt";
        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class AssemblyUpdater
    {
        private Version _latestVersion;

        public abstract string FilePath { get; }

        public abstract string VersionsServerPath { get; }

        [CanBeNull]
        public Version CurrentVersion => !File.Exists(FilePath)
            ? null
            : new Version(FileVersionInfo.GetVersionInfo(FilePath).FileVersion);

        public Version LatestVersion =>
            _latestVersion ?? (_latestVersion = VersionUtilities.GetLatestFileVersion(VersionsServerPath));

        protected abstract void Update();

        protected void DeleteCurrent()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
