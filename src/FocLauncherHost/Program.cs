﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows;
using FocLauncher;
using FocLauncherHost.ExceptionHandling;
using Microsoft.Win32;

namespace FocLauncherHost
{
    public static class Program
    {
        private static readonly string ApplicationBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FoC Launcher");

        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static void Main()
        {
            if (!Get48FromRegistry())
                Environment.Exit(0);

            SetAndInitApplicationBasePath();

            RunHostApplication();

            //var splashDomain = AppDomain.CreateDomain("BootstrapDomain");
            //// Gotta catch 'em all.
            //AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionReceived;
            //splashDomain.AssemblyResolve += LauncherAppDomainResolveAssembly;

            //try
            //{
            //    splashDomain.DoCallBack(RunHostApplication);
            //}
            //finally
            //{
            //    AppDomain.Unload(splashDomain);
            //}

            var s = new AppDomainSetup
            {
                ApplicationName = "FoC Launcher",
                ApplicationBase = LauncherConstants.ApplicationBasePath,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            var launcher = AppDomain.CreateDomain("LauncherDomain", null, s);
            var location = Path.Combine(LauncherConstants.ApplicationBasePath, "FocLauncher.dll");
            var t = (IsolatingLauncherBootstrapper) launcher.CreateInstanceFromAndUnwrap(location, 
                typeof(IsolatingLauncherBootstrapper).FullName);

            //launcher.AssemblyResolve += LauncherAppDomainResolveAssembly;
            try
            {
                t.StartLauncherApplication();
                //launcher.DoCallBack(StartLauncher);
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledExceptionReceived;
                AppDomain.Unload(launcher);
            }
        }

        private static void SetAndInitApplicationBasePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!PathUtilities.UserHasDirectoryAccessRights(appDataPath, FileSystemRights.CreateDirectories))
            {
                var exception = new IOException($"Permission on '{appDataPath}' denied: Creating a new directory");
                ShowExceptionDialogAndExit(exception);
            }

            Environment.SetEnvironmentVariable(LauncherConstants.ApplicationBaseVariable, ApplicationBasePath, EnvironmentVariableTarget.Process);

            if (!Directory.Exists(LauncherConstants.ApplicationBasePath))
                Directory.CreateDirectory(LauncherConstants.ApplicationBasePath);
        }

        private static void RunHostApplication()
        {
            var app = new HostApplication();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Run(new SplashScreen());
            app.Shutdown(0);
        }

        private static void StartLauncher()
        {
            // Make sure we reference to FocLauncher.dll here the first time. Otherwise the update code might break because the assembly could not be resolved.
            var app = new LauncherApp();
            app.Run();
        }

        private static void ShowExceptionDialogAndExit(Exception exception, bool exit = true)
        {
            new ExceptionWindow(exception).ShowDialog();
            if (exit)
                Environment.Exit(exception.HResult);
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

        private static Assembly LauncherAppDomainResolveAssembly(object sender, ResolveEventArgs args)
        {
            var fields = args.Name.Split(',');
            var name = fields[0];
            var culture = fields[2];

            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;

            var files = Directory.EnumerateFiles(LauncherConstants.ApplicationBasePath, "*.dll", SearchOption.TopDirectoryOnly);
            var dll = files.FirstOrDefault(x => $"{name}.dll".Equals(Path.GetFileName(x)));
            return dll == null ? null : Assembly.LoadFile(dll);
        }
    }
}
