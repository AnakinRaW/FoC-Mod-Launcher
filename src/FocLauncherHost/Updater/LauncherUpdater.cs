namespace FocLauncherHost.Updater
{
    internal class LauncherUpdater : AssemblyUpdater
    {
        public override string AssemblyName => "FocLauncher.dll";

        protected override VersionType VersionType => VersionType.Launcher;
    }
}
