using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
            
            if (actionQueue.Any())
            {
                await WaitForMainWindow();
                await Current.Dispatcher.Invoke(async () =>
                {
                    var twd = WaitDialogFactory.CreateInstance();
                    twd.StartWaitDialog("FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....", 2, false, true);
                    try
                    {
                        await Task.Delay(5000);
                        //foreach (var func in actionQueue) 
                        //    await func();
                    }
                    finally
                    {
                        twd?.EndWaitDialog(out _);
                    }
                }, DispatcherPriority.Background);

                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                //ThreadHelper.JoinableTaskFactory.Run("FoC Launcher",
                //    "Please wait while the launcher is loading an update.", "Updating....",
                //    async () => { await Task.Delay(5000); }, 2);
            }

            //MainWindow?.Dispatcher.InvokeShutdown();
            //Dispatcher.InvokeShutdown();
            Shutdown();
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

        private async Task WaitForMainWindow()
        {
            if (Current.MainWindow != null && Current.MainWindow.IsVisible)
                return;
            await Task.Delay(100);
        }
    }
}