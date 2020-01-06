using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Threading;
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

            if (!actionQueue.Any())
            {
                ThreadHelper.Generic.Invoke(() => MainWindow?.Show());
                await WaitDialogHelper.RunWithWaitDialog(async () =>
                {
                    await Task.Delay(5000);
                    //foreach (var action in actionQueue)
                    //    await action();
                }, "FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....", 2, true);
            }

            Dispatcher.InvokeShutdown();
            MainWindow?.Dispatcher.InvokeShutdown();
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
            if (currentVersion == null || latestVersion > currentVersion)
                actionQueue.Enqueue(updater.Update);
        }

        private static void ExtractAssembly(string assemblyName)
        {
            var extractor = new ResourceExtractor("Library");
            extractor.ExtractFilesIfRequired(AppDataPath, new[] { assemblyName });
        }
    }
}