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
        // TODO: This should be a flexible server, not the final
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";

        static HostApplication()
        {
            // Since FocLauncher.Threading.dll and Microsoft.VisualStudio.Utilities.dll are used by the WaitWindow AppDomain we need to have them on disk
            // Make sure not to use async file writing as we need to block the app until necessary assembly are written to disk
            // TODO: Do not extract Launcher and Theming here
            AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath, 
                "FocLauncher.Threading.dll", "Microsoft.VisualStudio.Utilities.dll");
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new SplashScreen();
            PrepareAndUpdateLauncherAsync().Forget();
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
    }
}