using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FocLauncher.Core;
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
            launcher.AssemblyResolve += LauncherAppDomainResolveAssembly;
            try
            {
                launcher.DoCallBack(StartLauncher);
            }
            catch (Exception e)
            {
                //TODO: Make this fancy :)
                MessageBox.Show(e.Message);
            }
            finally
            {
                AppDomain.Unload(launcher);
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

        private static void StartLauncher()
        {
            var app = new LauncherApp();
            app.Run();
        }

        private static void StartBootstrapperApp()
        {
            var app = new BootstrapperApp();
            var w = new SplashScreen();
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
