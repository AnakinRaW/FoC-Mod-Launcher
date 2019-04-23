using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace FocLauncher.Game
{
    public static class Steam
    {
        public static string SteamExePath
        {
            get
            {
                try
                {
                    return Registry.CurrentUser.CreateSubKey("Software\\Valve\\Steam", RegistryKeyPermissionCheck.ReadSubTree)?.GetValue("SteamExe", null).ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public static bool IsSteamInstalled(out string path)
        {
            path = SteamExePath;
            return !string.IsNullOrEmpty(path);
        }

        public static bool IsSteamRunning()
        {
            if (!IsSteamInstalled(out _))
                return false;

            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                var steamKey = registry.OpenSubKey("Software\\Valve\\Steam\\ActiveProcess", false);
                if (steamKey == null)
                    return false;
                var pid = (int)steamKey.GetValue("pid");
                if (pid == 0)
                    return false;
                if (ProcessHelper.GetProcessByPid(pid) == null)
                    return false;
                return true;
            }
        }

        public static bool IsUserLoggedIn(out int userId)
        {
            userId = -1;
            if (!IsSteamInstalled(out _))
                return false;

            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                var steamKey = registry.OpenSubKey("Software\\Valve\\Steam\\ActiveProcess", false);
                if (steamKey == null)
                    return false;
                userId = (int)steamKey.GetValue("ActiveUser");
                return userId > 0;
            }
        }

        public static void WaitUserChanged(int ticks)
        {
            IsUserLoggedIn(out var lastUserId);
            if (lastUserId == -1)
                return;

            var tick = 0;
            while (tick++ < tick)
            {
                IsUserLoggedIn(out var curretnUser);
                if (curretnUser == lastUserId)
                    continue;
                return;
            }
        }


        public static bool IsSteamGoldPackInstalled()
        {
            if (!IsSteamInstalled(out _))
                return false;

            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                return registry.OpenSubKey("Software\\Valve\\Steam\\Apps\\32470", false) != null ||
                       registry.OpenSubKey("Software\\Valve\\Steam\\Apps\\32472", false) != null;
            }
        }

        internal static void StartSteam()
        {
            if (!IsSteamInstalled(out var steamPath))
                return;
            var process = new Process
            {
                StartInfo =
                {
                    FileName = steamPath,
                    UseShellExecute = false
                }
            };
            process.Start();
            WaitUserChanged(3000);
        }
    }
}
