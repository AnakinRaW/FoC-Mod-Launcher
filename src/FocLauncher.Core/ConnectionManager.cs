using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher.NativeMethods;

namespace FocLauncher
{
    public class ConnectionManager
    {
        private const string GoogleDNSAddress = "8.8.8.8";
        private const int RefreshStateSeconds = 8;
        private const int PingTimeoutMilliseconds = 1000;
        private static readonly IPAddress PingServer = IPAddress.Parse(GoogleDNSAddress);
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private static ConnectionManager? _instance;

        private DateTime _lastStateChange;
        private bool _connected;
        
        public static ConnectionManager Instance => _instance ??= new ConnectionManager();

        private bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                _lastStateChange = DateTime.Now;
            }
        }

        private ConnectionManager()
        {
            InvalidateAsync().ConfigureAwait(false);
        }


        public async Task<bool> CheckConnectionAsync()
        {
            var elapsedSeconds = (DateTime.Now - _lastStateChange).TotalSeconds;
            if (elapsedSeconds > RefreshStateSeconds)
                await InvalidateAsync();
            return Connected;

        }

        public bool CheckConnection()
        {
            return CheckConnectionAsync().Result;
        }

        private async Task InvalidateAsync()
        {
            await SemaphoreSlim.WaitAsync();

            try
            {
                if (!HasPhysicalConnection())
                    Connected = false;
                else
                {
                    using var ping = new Ping();
                    try
                    {
                        var result = await ping.SendPingAsync(PingServer, PingTimeoutMilliseconds);
                        Connected = result.Status == IPStatus.Success;
                    }
                    catch (PingException)
                    {
                        Connected = false;
                    }
                }
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private static bool HasPhysicalConnection()
        {
            var hasConnection = WiniNet.InternetGetConnectedState(out var flags);
            return hasConnection && flags.HasFlag(WiniNet.ConnectionStates.LAN);
        }
    }
}