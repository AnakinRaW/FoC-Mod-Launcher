using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FocLauncherApp.Updater;
using FocLauncherApp.Utilities;
using FocLauncherApp.WaitDialog;

namespace FocLauncherApp
{
    public class BootstrapperApp : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";

        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
                await Current.Dispatcher.Invoke(() => UpdateAsync(actionQueue), DispatcherPriority.Background);
            }

            //MainWindow?.Dispatcher.InvokeShutdown();
            //Dispatcher.InvokeShutdown();
            Shutdown();
        }

        private async Task UpdateAsync(IEnumerable<Func<Task>> actions)
        {
            var twd = WaitDialogServiceWrapper.CreateInstance();
            bool cancelled;
            twd.StartWaitDialog("FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....",
                true, 2, true, _cancellationTokenSource);
            try
            {
                //await Task.Delay(5000, _cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                }

                //foreach (var func in actionQueue) 
                //    await func();
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                twd.EndWaitDialog(out cancelled);
            }

            if (cancelled)
                return;

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