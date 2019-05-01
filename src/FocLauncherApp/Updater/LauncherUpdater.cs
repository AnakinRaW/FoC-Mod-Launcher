using System;
using System.Diagnostics;
using System.IO;
using FocLauncherApp.Properties;

namespace FocLauncherApp.Updater
{
    internal class LauncherUpdater : AssemblyUpdater
    {
        public override string FilePath => Path.Combine(BootstrapperApp.AppDataPath, "FocLauncher.Core.dll");

        protected override VersionType VersionType => VersionType.Launcher;

        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }

    internal class ThemeUpdater : AssemblyUpdater
    {
        public override string FilePath => Path.Combine(BootstrapperApp.AppDataPath, "FocLauncher.Theming.dll");

        protected override VersionType VersionType => VersionType.Theme;

        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class AssemblyUpdater
    {
        private Version _latestVersion;

        public abstract string FilePath { get; }

        public virtual string VersionsServerPath => "master/Releases/AvailableUpdates.txt";

        protected abstract VersionType VersionType { get;  }

        [CanBeNull]
        public Version CurrentVersion => !File.Exists(FilePath)
            ? null
            : new Version(FileVersionInfo.GetVersionInfo(FilePath).FileVersion);

        public Version LatestVersion =>
            _latestVersion ?? (_latestVersion = VersionUtilities.GetLatestVersion(VersionsServerPath, VersionType));

        protected abstract void Update();

        protected void DeleteCurrent()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
