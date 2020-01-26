using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FocLauncher;
using FocLauncherHost.Properties;

namespace FocLauncherHost.Updater
{
    internal abstract class AssemblyUpdater
    {
        private Version _latestVersion;

        public abstract string AssemblyName { get; }

        public virtual string FilePath => Path.Combine(LauncherConstants.ApplicationBasePath, AssemblyName);

        public virtual string VersionsServerPath => "master/Releases/AvailableUpdates.txt";

        protected abstract VersionType VersionType { get;  }

        // TODO: Use assembly Version
        [CanBeNull]
        public Version CurrentVersion
        {
            get
            {
                if (!File.Exists(FilePath) || File.ReadAllBytes(FilePath).Length == 0)
                    return null;
                return new Version(FileVersionInfo.GetVersionInfo(FilePath).FileVersion);
            }
        }

        public Version LatestVersion =>
            _latestVersion ??= VersionUtilities.GetLatestVersion(VersionsServerPath, VersionType);

        public async Task Update()
        {
            var server = new UpdateServer(HostApplication.ServerUrl);
            if (await server.IsRunning() == false)
                return;
            var tempFilePath = FilePath + ".new";
            await server.DownloadFile(BuildDownloadSourcePath(), tempFilePath);
            if (!File.Exists(tempFilePath))
                return;
            DeleteCurrent();
            File.Move(tempFilePath, FilePath);
        }

        protected void DeleteCurrent()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        protected virtual string BuildDownloadSourcePath()
        {
            return $"master/Releases/{VersionType}/{LatestVersion}/{AssemblyName}";
        }
    }
}