using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
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

        public bool IsRunning
        {
            get
            {
                var pid = ProcessId;
                if (!pid.HasValue)
                    return false;
                if (pid.Value == 0)
                    return false;
                return ProcessHelper.GetProcessByPid(pid.Value) != null;
            }
        }

        internal bool IsUserLoggedIn => ActiveUser.HasValue && ActiveUser.Value != 0;

        private int? ActiveUser
        {
            get
            {
                _registry.GetValue("ActiveUser", "ActiveProcess", out int? user);
                return user;
            }
            set => _registry.WriteValue("ActiveUser", "ActiveProcess", value);
        }

        private int? ProcessId
        {
            get
            {
                _registry.GetValue("pid", "ActiveProcess", out int? pid);
                return pid;
            }
        }
        
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
        }

        public async Task WaitSteamRunningAndLoggedInAsync(CancellationToken token = default)
        {
            ThrowIfSteamNotInstalled();

            var running = IsRunning;
            if (!running)
            {
                // Required because a taskmgr kill does not reset this value, so we have to do this manually
                ActiveUser = 0;
                await WaitSteamRunningAsync(token);
            }
            if (IsUserLoggedIn)
                return;
            await WaitSteamUserLoggedInAsync(token);
        }

        private async Task WaitSteamRunningAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsRunning)
                return;
            
            while (!token.IsCancellationRequested && !IsRunning)
            {
                var k = _registry.GetKey("ActiveProcess");
                await k.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
            }
        }

        public async Task WaitSteamUserLoggedInAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsUserLoggedIn)
                return;

            while (!token.IsCancellationRequested && !IsUserLoggedIn)
            {
                var k = _registry.GetKey("ActiveProcess");
                await k.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
            }
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