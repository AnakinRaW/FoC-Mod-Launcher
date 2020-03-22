using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Input;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncherHost.Controls;
using Microsoft.Win32;
using NLog;

namespace FocLauncherHost
{
    public static class Program
    {
        private static readonly string ApplicationBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FoC Launcher");

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static void Main(string[] args)
        {
            if (!Get48FromRegistry())
                Environment.Exit(0);

            // Gotta catch 'em all.
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionReceived;

            var startOption = ExternalUpdaterResult.NoUpdate;
            if (args.Length >= 1)
            {
                var argument = args[0];
                if (int.TryParse(argument, out var value) && Enum.IsDefined(typeof(ExternalUpdaterResult), value)) 
                    startOption = (ExternalUpdaterResult) value;
            }
                
            SetAndInitApplicationBasePath(startOption);
            if (startOption == ExternalUpdaterResult.NoUpdate)
                NLogUtils.DeleteOldLogFile();
            NLogUtils.SetLoggingForAppDomain();

            Logger.Debug($"Started FoC Launcher with arguments: {startOption}");

            ShowSplashScreen();
            StartLauncher();
            LogManager.Shutdown();
        }

        private static void SetAndInitApplicationBasePath(ExternalUpdaterResult launchOption)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!PathUtilities.UserHasDirectoryAccessRights(appDataPath, FileSystemRights.CreateDirectories))
            {
                var exception = new IOException($"Permission on '{appDataPath}' denied: Creating a new directory");
                ShowExceptionDialogAndExit(exception);
            }

            Environment.SetEnvironmentVariable(LauncherConstants.ApplicationBaseVariable, ApplicationBasePath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(LauncherConstants.ExecutablePathVariable, Directory.GetCurrentDirectory(), EnvironmentVariableTarget.Process);

            if ((launchOption == ExternalUpdaterResult.UpdateFailedNoRestore || (Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                && Directory.Exists(LauncherConstants.ApplicationBasePath))
                Directory.Delete(LauncherConstants.ApplicationBasePath, true);

            if (!Directory.Exists(LauncherConstants.ApplicationBasePath))
                Directory.CreateDirectory(LauncherConstants.ApplicationBasePath);
        }

        private static void ShowSplashScreen()
        {
            // New AppDomain required so the initial AppDomain can watch for unhandled exceptions and prompt the error window
            var splashDomain = AppDomain.CreateDomain("SplashScreenDomain");
            try
            {
                splashDomain.DoCallBack(RunHostApplication);
            }
            finally
            {
                Logger.Info("Unloading Splash Screen AppDomain");
                AppDomain.Unload(splashDomain);
            }
        }

        private static void StartLauncher()
        {
            var launcherDomain = CreateLauncherAppDomain();
            var launcherBootstrapper = CreateLauncherBootstrapper(launcherDomain);
            try
            {
                Logger.Info("Starting launcher bootstrapper in new AppDomain");
                launcherBootstrapper.StartLauncherApplication();
            }
            finally
            {
                Logger.Info("Unloading launcher appdomain");
                launcherBootstrapper.Dispose();
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledExceptionReceived;
                AppDomain.Unload(launcherDomain);
            }
        }

        private static void RunHostApplication()
        {
            NLogUtils.SetLoggingForAppDomain();
            Logger.Info("Starting Splash Screen on new AppDomain");
            var app = new HostApplication();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Run();
            app.Shutdown(0);
        }

        private static void ShowExceptionDialogAndExit(Exception exception, bool exit = true)
        {
            new ExceptionWindow(exception).ShowDialog();
            if (exit)
            {
                Logger.Fatal(exception, "Application terminated with error.");
                Environment.Exit(exception.HResult);
            }
            Logger.Error(exception);
        }

        private static bool Get48FromRegistry()
        {
            using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\");
            if (ndpKey?.GetValue("Release") != null)
            {
                if (CheckFor48DotVersion((int) ndpKey.GetValue("Release")))
                    return true;
                MessageBox.Show("Required .NetFramework Version 4.8 was not found");
                return false;
            }

            MessageBox.Show("Required .NetFramework Version 4.8 was not found");
            return false;
        }

        private static bool CheckFor48DotVersion(int releaseKey)
        {
            return releaseKey >= 528040;
        }

        private static void OnUnhandledExceptionReceived(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception) 
                ShowExceptionDialogAndExit(exception, e.IsTerminating);
        }

        private static void ThrowIFileNotFound(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Could not find {Path.GetFileName(filePath)}", filePath);
        }

        private static AppDomain CreateLauncherAppDomain()
        {
            var s = new AppDomainSetup
            {
                ApplicationName = "FoC Launcher",
                ApplicationBase = LauncherConstants.ApplicationBasePath,
                //LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            return AppDomain.CreateDomain("LauncherDomain", null, s);
        }

        private static IsolatingLauncherBootstrapper CreateLauncherBootstrapper(AppDomain appDomain)
        {
            Logger.Info("Creating launcher bootstrapper");
            var location = Path.Combine(LauncherConstants.ApplicationBasePath, "FocLauncher.dll");
            ThrowIFileNotFound(location);
            return (IsolatingLauncherBootstrapper)appDomain.CreateInstanceFromAndUnwrap(location,
                typeof(IsolatingLauncherBootstrapper).FullName);
        }
    }
}
