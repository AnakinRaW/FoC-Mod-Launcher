namespace FocLauncherHost.Updater
{
    internal class ThemeUpdater : AssemblyUpdater
    {
        public override string AssemblyName => "FocLauncher.Theming.dll";

        protected override VersionType VersionType => VersionType.Theme;
    }
}