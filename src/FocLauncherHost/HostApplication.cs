using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncher.WaitDialog;
using FocLauncherHost.Updater;
using FocLauncherHost.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncherHost
{
    public class HostApplication : Application
    {
        static HostApplication()
        {
            // Since FocLauncher.Threading.dll and Microsoft.VisualStudio.Utilities.dll are used by the WaitWindow AppDomain we need to have them on disk
            // Make sure not to use async file writing as we need to block the app until necessary assembly are written to disk
            // TODO: Do not extract Launcher and Theming here
            AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath, 
                "FocLauncher.Threading.dll", "Microsoft.VisualStudio.Utilities.dll");
        }

        // TODO: This should be a flexible server, not the final
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";



        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ConnectionManager _connectionManager;

        public HostApplication()
        {
            _connectionManager = ConnectionManager.Instance;
        }

        protected override /*async*/ void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new SplashScreen();
            PrepareAndUpdateLauncherAsync().Forget();

            //await AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath, "FocLauncher.dll", "FocLauncher.Theming.dll");

            //await new UpdateManager().CheckAndPerformUpdateAsync();

            //var actionQueue = new Queue<Func<Task>>();

            //var launcherUpdater = new LauncherUpdater();
            //await CheckForUpdate(launcherUpdater, actionQueue);
            //var themeUpdater = new ThemeUpdater();
            //await CheckForUpdate(themeUpdater,  actionQueue);
            
            //if (!actionQueue.Any())
            //{
            //    await WaitForMainWindow();
            //    await Current.Dispatcher.Invoke(async () => await UpdateAsync(actionQueue), DispatcherPriority.Background);
            //}

            //MainWindow?.Dispatcher.InvokeShutdown();
            //Dispatcher.InvokeShutdown();
            //Shutdown();
        }

        internal async Task PrepareAndUpdateLauncherAsync()
        {
            var t = AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath, "FocLauncher.dll", "FocLauncher.Theming.dll");
            
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var data = new WaitDialogProgressData("Please wait while the launcher is loading an update.", "Updating....", null, true);

                var s = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, TimeSpan.FromSeconds(2));
                try
                {
                    Task.WhenAll(t, Task.Delay(200)).ContinueWith(async task => await ShowMainWindowAsync(), s.UserCancellationToken,
                        TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()).Forget();
                    await new UpdateManager().CheckAndPerformUpdateAsync();
                    await Task.Delay(5000, s.UserCancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    s.Dispose();
                    // TODO: Event of the update manager
                    await HideSplashScreenAnimatedAsync();
                    Shutdown();
                }
            });
        }

        private async Task ShowMainWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            MainWindow?.Show();
        }

        private async Task HideSplashScreenAnimatedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (MainWindow is SplashScreen splashScreen && splashScreen.IsVisible)
            {
                await Task.Delay(500);
                await splashScreen.HideAnimationAsync();
            }
        }


        private async Task CheckForUpdate(AssemblyUpdater updater, Queue<Func<Task>> actionQueue)
        {
            var hasConnection = await _connectionManager.CheckConnectionAsync();

            var currentVersion = updater.CurrentVersion;

            //TODO: Extract always when the embedded version is higher
            if (!hasConnection && currentVersion == null)
            {
                actionQueue.Enqueue(async () => await AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath, updater.AssemblyName));
                //actionQueue.Enqueue(async () => await Task.Run(() => ResourceExtractor.ExtractAssembly(LauncherConstants.ApplicationBasePath, updater.AssemblyName)));
                return;
            }

            // TODO: Async
            var latestVersion = updater.LatestVersion;
            if (currentVersion == null || latestVersion > currentVersion)
                actionQueue.Enqueue(updater.Update);
        }
        
        private async Task UpdateAsync(IEnumerable<Func<Task>> actions)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var data = new WaitDialogProgressData("Please wait while the launcher is loading an update.", "Updating....", null, true);


                var s = WaitDialogFactory.Instance.StartWaitDialog("123", data, TimeSpan.FromSeconds(2));
                try
                {
                    foreach (var func in actions)
                        await func();

                    await Task.Delay(50000, s.UserCancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    s.Dispose();
                }
            });


            //var twd = WaitDialogServiceWrapper.CreateInstance();
            //bool cancelled;
            //twd.StartWaitDialog("FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....",
            //    true, 2, true, _cancellationTokenSource);
            //try
            //{
            //    //await Task.Delay(5000, _cancellationTokenSource.Token);
            //    if (_cancellationTokenSource.IsCancellationRequested)
            //    {
            //    }

            //    //foreach (var func in actionQueue) 
            //    //    await func();
            //}
            //catch (TaskCanceledException)
            //{
            //}
            //finally
            //{
            //    twd.EndWaitDialog(out cancelled);
            //}

            //if (cancelled)
            //    return;

        }

        private static async Task WaitForMainWindow()
        {
            if (Current.MainWindow != null && Current.MainWindow.IsVisible)
                return;
            await Task.Delay(100);
        }
    }
}