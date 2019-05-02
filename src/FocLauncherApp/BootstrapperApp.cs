using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Updater;
using FocLauncherApp.Utilities;
using FocLauncherApp.WaitDialog;

namespace FocLauncherApp
{
    public class BootstrapperApp : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var actionQueue = new Queue<Func<Task>>();

            var launcherUpdater = new LauncherUpdater();
            CheckForUpdate(launcherUpdater, in actionQueue);
            var themeUpdater = new ThemeUpdater();
            CheckForUpdate(themeUpdater, in actionQueue);

            await WaitDialogHelper.RunWithWaitDialog(async () =>
            {
                foreach (var action in actionQueue)
                    await action();
            }, "FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....", 2, true);

            Shutdown(0);
        }

        private void CheckForUpdate(AssemblyUpdater updater, in Queue<Func<Task>> actionQueue)
        {
            var hasConnection = NativeMethods.NativeMethods.InternetGetConnectedState(out _, 0);
            var currentVersion = updater.CurrentVersion;

            if (!hasConnection && currentVersion == null)
            {
                actionQueue.Enqueue(async () => await Task.Run(() => ExtractAssembly(updater.AssemblyName)));
                return;
            }
            var latestVersion = updater.LatestVersion;
            // TODO: change >=  to >
            if (currentVersion == null || latestVersion >= currentVersion)
                actionQueue.Enqueue(() => AsyncMethod3("1213"));
        }

        private static void ExtractAssembly(string assemblyName)
        {
            var extractor = new ResourceExtractor("Library");
            extractor.ExtractFilesIfRequired(AppDataPath, new[] { assemblyName });
        }

        private static async Task AsyncMethod3(string t)
        {
            try
            {
                //await Task.Delay(1000);
                //await Task.Delay(1000);
                //await Task.Delay(1000);
                //MessageBox.Show(t);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}