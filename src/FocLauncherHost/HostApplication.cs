using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncher.WaitDialog;
using FocLauncherHost.Controls;
using FocLauncherHost.Utilities;
using Microsoft.VisualStudio.Threading;
using NLog;
using TaskBasedUpdater;
using TaskBasedUpdater.Elevation;

namespace FocLauncherHost
{
    public class HostApplication : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";

        private readonly AsyncManualResetEvent _canCloseApplicationEvent = new AsyncManualResetEvent(false, true);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _waitWindowShown;

        private static readonly TimeSpan WaitSplashDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan WaitWindowDelay = TimeSpan.FromSeconds(WaitSplashDelay.Seconds + 2);

        internal static ManualResetEvent SplashVisibleResetEvent { get; } = new ManualResetEvent(false);

        static HostApplication()
        {
            // Since FocLauncher.Threading.dll and Microsoft.VisualStudio.Utilities.dll are used by the WaitWindow AppDomain we need to have them on disk
            // Make sure not to use async file writing as we need to block the app until necessary assembly are written to disk
            AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath,
                "FocLauncher.Threading.dll", "Microsoft.VisualStudio.Utilities.dll");
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new SplashScreen();
            WaitAndShutdownAsync().Forget();
            PrepareAndUpdateLauncherAsync().Forget();
        }
        
        internal async Task PrepareAndUpdateLauncherAsync()
        {
            try
            {
                await AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath,
                    "FocLauncher.dll", "FocLauncher.Theming.dll", LauncherConstants.ElevatorFileName);

                Task.Delay(WaitSplashDelay).ContinueWith(async _ => await ShowMainWindowAsync(), default,
                    TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext()).Forget();

                await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    var data = new WaitDialogProgressData("Please wait while the launcher is downloading an update.",
                        isCancelable: true);

                    var session = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, WaitWindowDelay);
                    SetWhenWaitDialogIsShownAsync(WaitWindowDelay, session.UserCancellationToken).Forget();

                    session.UserCancellationToken.Register(OnUserCancelled);

                    var cts = CancellationTokenSource.CreateLinkedTokenSource(session.UserCancellationToken);

                    UpdateInformation updateInformation = null;
                    try
                    {
                        var updateManager =
                            new FocLauncherUpdaterManager(@"C:\Users\Anakin\OneDrive\launcherUpdate.xml");
                        updateInformation = await updateManager.CheckAndPerformUpdateAsync(cts.Token);
                        Logger.Info($"Finished automatic update with result {updateInformation}");
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Info("Download Operation cancelled");
                    }
                    finally
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                        session.Dispose();
                        cts.Dispose();
                    }

                    ReportUpdateResult(updateInformation);
                });
            }
            catch (ElevationRequireException)
            {
                try
                {
                    Elevator.RestartElevated();
                }
                catch (Exception e)
                {
                    // The elevation was not accepted by the user
                    if (e is Win32Exception && e.HResult == -2147467259)
                    {
                        MessageBox.Show("The update failed");
                        return;
                    }
                    LogAndShowException(e);
                }
            }
            catch (Exception e)
            {
                LogAndShowException(e);
            }
            finally
            {
                _canCloseApplicationEvent.Set();
            }
        }

        private async Task SetWhenWaitDialogIsShownAsync(TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);
            if (token.IsCancellationRequested)
                return;
            _waitWindowShown = true;
        }

        private void ReportUpdateResult(UpdateInformation updateInformation)
        {

            if (updateInformation != null)
            {
                if (updateInformation.RequiresUserNotification && _waitWindowShown 
#if DEBUG
                   || true
#endif
                    )
                {
                    MessageBox.Show($"Updating finished with result: {updateInformation.Result}\r\n" +
                                    $"Message: {updateInformation.Message}", "FoC Launcher");
                }
                
            }
        }

        private void OnUserCancelled()
        {
            // This runs before the exception is getting caught.
        }

        private async Task WaitAndShutdownAsync()
        {
            await _canCloseApplicationEvent.WaitAsync();
            await HideSplashScreenAnimatedAsync();
            Shutdown();
        }

        private async Task ShowMainWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            MainWindow?.Show();
            SplashVisibleResetEvent.Set();
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

        private void LogAndShowException(Exception e)
        {
            Logger.Error(e, e.Message);
            var realException = e.TryGetWrappedException() ?? e;
            new ExceptionWindow(realException).ShowDialog();
        }
    }
}