using System;
using System.Diagnostics;
using Microsoft.Win32;
using NLog;

namespace FocLauncher.Game
{
    public class SteamClient
    {
        private readonly SteamRegistry _registry;
        private static SteamClient _instance;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SteamClient Instance => _instance ??= new SteamClient();

        internal string? SteamExePath { get; private set; }

        public bool Installed => !string.IsNullOrEmpty(SteamExePath);

        private SteamClient()
        {
            _registry = new SteamRegistry();
            Initialize();
        }

        private void Initialize()
        {
            Logger.Trace("Initializing Steam Client");
            try
            {
                if (_registry.GetValue<string>("SteamExe", out var path))
                {
                    Logger.Trace($"Steam installed under location: '{path}'");
                    SteamExePath = path;
                }
                else
                    Logger.Warn("Steam is not installed");
            }
            catch (Exception)
            {
                Logger.Warn("Erro checking whether steam is installed");
            }
        }

        public bool IsSteamRunning()
        {
            ThrowIfSteamNotInstalled();
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

        public bool IsUserLoggedIn(out int userId)
        {
            ThrowIfSteamNotInstalled();
            userId = -1;

            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            {
                var steamKey = registry.OpenSubKey("Software\\Valve\\Steam\\ActiveProcess", false);
                if (steamKey == null)
                    return false;
                userId = (int)steamKey.GetValue("ActiveUser");
                return userId > 0;
            }
        }

        public void WaitUserChanged(int ticks)
        {
            ThrowIfSteamNotInstalled();
            IsUserLoggedIn(out var lastUserId);
            if (lastUserId > 0)
                return;

            var tick = 0;
            while (tick++ < tick)
            {
                IsUserLoggedIn(out var currentUser);
                if (currentUser == 0 || currentUser == lastUserId)
                    continue;
                return;
            }
        }


        public bool IsGameInstalled(int gameId)
        {
            ThrowIfSteamNotInstalled();
            return _registry.HasPath($"Apps\\{gameId}");
        }

        public void StartGame(int gameId)
        {
            ThrowIfSteamNotInstalled();
            if (!IsGameInstalled(gameId))
                throw new SteamClientException($"The game with the ID: {gameId} does not exists");
            Process.Start($"steam://rungameid/{gameId}");
        }

        internal void StartSteam()
        {
            ThrowIfSteamNotInstalled();
            var process = new Process
            {
                StartInfo =
                {
                    FileName = SteamExePath,
                    UseShellExecute = false
                }
            };
            process.Start();
            WaitUserChanged(3000);
        }

        protected void ThrowIfSteamNotInstalled()
        {
            if (!Installed)
                throw new SteamClientException("Steam is not installed!");
        }

        private class SteamRegistry : RegistryHelper
        {
            protected override string RegistryBasePath => "Software\\Valve\\Steam";
            protected override RegistryKey RootRegistryKey => Registry.CurrentUser;
        }
    }
}