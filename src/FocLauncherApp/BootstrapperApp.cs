using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Updater;
using FocLauncherApp.WaitDialog;
using ResourceExtractor = FocLauncherApp.Utilities.ResourceExtractor;
using ResourceExtractorException = FocLauncherApp.Utilities.ResourceExtractorException;

namespace FocLauncherApp
{
    public class BootstrapperApp : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var launcherUpdater = new LauncherUpdater();

            var result = CheckForUpdate(launcherUpdater);

            switch (result)
            {
                case CheckUpdateResult.Load:
                    LoadAssemblies();
                    break;
                case CheckUpdateResult.ExtractAndLoad:
                    ExtractAssemblies();
                    LoadAssemblies();
                    break;
                case CheckUpdateResult.Update:
                    //await WaitDialogHelper.RunWithWaitDialog(AsyncMethod3, "FoC Launcher",
                    //    "Please wait while the launcher is loading an update.", "Updating....", 2, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Shutdown(0);
        }

        private CheckUpdateResult CheckForUpdate(AssemblyUpdater updater)
        {
            var hasConnection = NativeMethods.NativeMethods.InternetGetConnectedState(out _, 0);
            var currentVersion = updater.CurrentVersion;

            if (!hasConnection)
            {
                if (currentVersion == null)
                    return CheckUpdateResult.ExtractAndLoad;
                if (currentVersion > Version.Parse("1.0.0.0"))
                    return CheckUpdateResult.ExtractAndLoad;
                return CheckUpdateResult.Load;
            }

            var latestVersion = updater.LatestVersion;
            if (currentVersion == null || latestVersion > currentVersion)
                return CheckUpdateResult.Update;
            return CheckUpdateResult.Load;
        }

        private void ExtractAssemblies()
        {
            var extractor = new ResourceExtractor("Library");
            try
            {
                extractor.ExtractFilesIfRequired(AppDataPath, new[] {""});
            }
            catch (ResourceExtractorException e)
            {
                MessageBox.Show("Error while extracting necessary .dll files:\r\n\r\n" + e.Message);
                Environment.Exit(0);
            }
        }

        private void LoadAssemblies()
        {

        }

        private static async Task AsyncMethod3()
        {
            try
            {
                await Task.Delay(1000);
                await Task.Delay(1000);
                await Task.Delay(1000);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private enum CheckUpdateResult
        {
            Load,
            ExtractAndLoad,
            Update
        }
    }
}