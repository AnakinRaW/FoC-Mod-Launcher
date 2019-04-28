using System;
using System.Windows;
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
            try
            {
                splashDomain.DoCallBack(StartBootstrapperApp);
            }
            finally
            {
                AppDomain.Unload(splashDomain);
            }

            var launcher = AppDomain.CreateDomain("LauncherDomain");
            try
            {
                launcher.DoCallBack(() =>
                {
                    var app = new BootstrapperApp();
                    var w = new Window();
                    app.Run(w);
                });
            }
            finally
            {
                AppDomain.Unload(launcher);
            }
        }

        private static void StartBootstrapperApp()
        {
            var app = new BootstrapperApp();
            var w = new Window();
            app.Run(w);
            app.Shutdown(0);
        }

        private static bool Get46FromRegistry()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey?.GetValue("Release") != null)
                {
                    if (CheckFor46DotVersion((int)ndpKey.GetValue("Release")))
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
