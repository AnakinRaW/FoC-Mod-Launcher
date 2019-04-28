using System;
using System.Diagnostics;
using System.IO;
using FocLauncher.Annotations;

namespace FocLauncherApp.Updater
{
    internal class LauncherUpdater : AssemblyUpdater
    {
        public override string FilePath => Path.Combine(BootstrapperApp.AppDataPath, "");
        public override string VersionsServerPath { get; }
        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class AssemblyUpdater
    {
        public abstract string FilePath { get; }

        public abstract string VersionsServerPath { get; }

        [CanBeNull]
        protected Version CurrentVersion => !File.Exists(FilePath)
            ? null
            : new Version(FileVersionInfo.GetVersionInfo(FilePath).FileVersion);

        protected abstract void Update();

        protected void DeleteCurrent()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
