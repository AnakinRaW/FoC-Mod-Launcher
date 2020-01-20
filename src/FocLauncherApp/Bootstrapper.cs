using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FocLauncher.Core;
using FocLauncherApp.ExceptionHandling;
using Microsoft.Win32;

namespace FocLauncherApp
{
    public static class Bootstrapper
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static void Main()
        {
            if (!Get46FromRegistry())
                Environment.Exit(0);

            var splashDomain = AppDomain.CreateDomain("BootstrapDomain");

            // Gotta catch 'em all.
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionReceived;

            try
            {
                splashDomain.DoCallBack(StartBootstrapperApp);
            }
            finally
            {
                AppDomain.Unload(splashDomain);
            }

            var s = new AppDomainSetup
            {
                ApplicationName = "FoC Launcher",
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };


            var launcher = AppDomain.CreateDomain("LauncherDomain", null, s);

            launcher.AssemblyResolve += LauncherAppDomainResolveAssembly;
            try
            {
                launcher.DoCallBack(StartLauncher);
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledExceptionReceived;
                AppDomain.Unload(launcher);
            }
        }

        private static void OnUnhandledExceptionReceived(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                new ExceptionWindow(exception).ShowDialog();
                if (e.IsTerminating)
                    Environment.Exit(exception.HResult);
            }       
        }

        private static Assembly LauncherAppDomainResolveAssembly(object sender, ResolveEventArgs args)
        {
            var fields = args.Name.Split(',');
            var name = fields[0];
            var culture = fields[2];

            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;

            var files = Directory.EnumerateFiles(BootstrapperApp.AppDataPath, "*.dll", SearchOption.TopDirectoryOnly);
            var dll = files.FirstOrDefault(x => $"{name}.dll".Equals(Path.GetFileName(x)));
            return dll == null ? null : Assembly.LoadFile(dll);
        }


        private static void StartBootstrapperApp()
        {
            var app = new BootstrapperApp();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Run(new SplashScreen());
            app.Shutdown(0);
        }

        private static void StartLauncher()
        {
            // Make sure we reference to FocLauncher.Core.dll here the first time. Otherwise the update code might break because the assembly could not be resolved.
            var app = new LauncherApp();
            app.Run();
        }

        private static bool Get46FromRegistry()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey?.GetValue("Release") != null)
                {
                    if (CheckFor46DotVersion((int) ndpKey.GetValue("Release")))
                        return true;
                    MessageBox.Show("Required .NetFramework Version 4.6 was not found");
                    return false;
                }

                MessageBox.Show("Required .NetFramework Version 4.6 was not found");
                return false;
            }
        }

        private static bool CheckFor46DotVersion(int releaseKey)
        {
            return releaseKey >= 393295;
        }
    }
}
