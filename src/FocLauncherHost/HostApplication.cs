using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncher.Threading;
using FocLauncherHost.Controls;
using FocLauncherHost.Utilities;
using Microsoft.VisualStudio.Threading;
using NLog;
using TaskBasedUpdater;

namespace FocLauncherHost
{
    public class HostApplication : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan WaitSplashDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan WaitProgressDelay = TimeSpan.FromSeconds(WaitSplashDelay.Seconds + 2);

        private readonly AsyncManualResetEvent _canCloseApplicationEvent = new AsyncManualResetEvent(false, true);
        private readonly ExternalUpdaterResult _startOption;

        internal static ManualResetEvent SplashVisibleResetEvent { get; } = new ManualResetEvent(false);

        internal SplashScreen SplashScreen { get; }

        internal HostApplication() : this(ExternalUpdaterResult.NoUpdate)
        {
            
        }

        internal HostApplication(ExternalUpdaterResult startOption)
        {
            _startOption = startOption;
            MainWindow = SplashScreen = new SplashScreen();
            SplashScreen.IsBeta = FocLauncherProduct.Instance.IsPreviewInstance;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WaitAndShutdownAsync().Forget();
            PrepareAndUpdateLauncherAsync().Forget();
        }
        
        internal async Task PrepareAndUpdateLauncherAsync()
        {
            try
            {
                if (_startOption == ExternalUpdaterResult.UpdateSuccess)
                    return;

                await AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath,
                    "FocLauncher.dll", 
                    "FocLauncher.Theming.dll", 
                    LauncherConstants.UpdaterFileName, 
                    "FocLauncher.Threading.dll", 
                    "Microsoft.VisualStudio.Utilities.dll");

                LogInstalledAssemblies();

                Task.Delay(WaitSplashDelay).ContinueWith(async _ => await ShowMainWindowAsync(), default,
                    TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext()).Forget();

                await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    SplashScreen.ProgressText = "Please wait while the launcher is downloading an update.";
                    SetWhenWaitDialogIsShownAsync(WaitProgressDelay, SplashScreen.CancellationToken).Forget();
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(SplashScreen.CancellationToken);
                    
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
                        cts.Dispose();
                    }

                    ReportUpdateResult(updateInformation);
                });
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

        private static void LogInstalledAssemblies()
        {
            var fileList = new List<string> {Environment.GetCommandLineArgs()[0]};
            var appBase = new DirectoryInfo(LauncherConstants.ApplicationBasePath);
            fileList.AddRange(appBase.GetFilesByExtensions(false, ".exe", ".dll").Select(x => x.FullName));

            Logger.Debug("****Installed Files*****");
            foreach (var file in fileList)
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(file).FileVersion;
                var fileName = Path.GetFileName(file);
                Logger.Debug($"\tFile: {fileName}, File-Version: {fileVersion}");
            }
            Logger.Debug("************************");
        }

        private async Task SetWhenWaitDialogIsShownAsync(TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);
            if (token.IsCancellationRequested)
                return;
            SplashScreen.IsProgressVisible = true;
        }

        private void ReportUpdateResult(UpdateInformation updateInformation)
        {
            if (updateInformation != null)
            {
                if (updateInformation.RequiresUserNotification && SplashScreen.IsProgressVisible
#if DEBUG
                   || true
#endif
                    )
                {
                    switch (updateInformation.Result)
                    {
                        case UpdateResult.Failed:
                            SplashScreen.ProgressText = "Update Failed";
                            break;
                        case UpdateResult.Success:
                            SplashScreen.ProgressText = "Update finished";
                            break;
                        case UpdateResult.SuccessRestartRequired:
                            SplashScreen.ProgressText = "Update requires restart";
                            break;
                        case UpdateResult.Cancelled:
                            SplashScreen.ProgressText = "Update cancelled";
                            break;
                    }

                    SplashScreen.Cancelable = false;
                    MessageBox.Show($"Updating finished with result: {updateInformation.Result}\r\n" +
                                    $"Message: {updateInformation.Message}", "FoC Launcher");
                }
                
            }
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
            if (SplashScreen.IsVisible)
            {
                SplashScreen.IsProgressVisible = false;
                await Task.Delay(500);
                await SplashScreen.HideAnimationAsync();
            }
        }

        private static void LogAndShowException(Exception e)
        {
            Logger.Error(e, e.Message);
            var realException = e.TryGetWrappedException() ?? e;
            new ExceptionWindow(realException).ShowDialog();
        }
    }
}