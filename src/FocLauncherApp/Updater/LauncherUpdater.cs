namespace FocLauncherApp.Updater
{
    internal class LauncherUpdater : AssemblyUpdater
    {
        public override string AssemblyName => "FocLauncher.Core.dll";

        protected override VersionType VersionType => VersionType.Launcher;
    }
}
