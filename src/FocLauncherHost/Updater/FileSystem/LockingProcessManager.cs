using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocLauncherHost.Updater.NativeMethods;
using NLog;

namespace FocLauncherHost.Updater.FileSystem
{
    internal class LockingProcessManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private int _sessionId;
        private string _sessionKey;
        private bool _registered;

        public bool IsDisposed { get; private set; }

        private LockingProcessManager(int sessionId, string sessionKey)
        {
            _sessionId = sessionId;
            _sessionKey = sessionKey;
        }

        ~LockingProcessManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static LockingProcessManager Create()
        {
            var strSessionKey = new StringBuilder(32);
            ThrowOnError(RestartMgr.RmStartSession(out var pSessionHandle, 0, strSessionKey));
            return new LockingProcessManager(pSessionHandle, strSessionKey.ToString());
        }

        public static IEnumerable<LockingProcessInfo> GetProcesses(IEnumerable<string> paths)
        {
            IEnumerable<LockingProcessInfo> source = null;
            try
            {
                using var manager = Create();
                manager.Register(paths);
                source = manager.GetProcesses();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to query processes with open handles (0x{ex.HResult:x8}): {ex.Message}");
            }

            if (source != null)
                return source.Where(process =>
                    process.ApplicationStatus != RestartMgr.ApplicationStatus.Stopped &&
                    process.ApplicationStatus != RestartMgr.ApplicationStatus.StoppedOther).ToArray();

            return Enumerable.Empty<LockingProcessInfo>();
        }

        public void Register(IEnumerable<string> files = null)
        {
            var fileNames = files?.ToArray();
            var count = fileNames?.Length ?? 0;
            if (count == 0)
                return;
            _registered = true;
            ThrowOnError(RestartMgr.RmRegisterResources(_sessionId, count, fileNames, 0, null, 0, null));
        }

        public IEnumerable<LockingProcessInfo> GetProcesses()
        {
            if (_registered)
            {
                var num = 0;
                var pnProcInfo = 0;
                RestartMgr.RmProcessInfo[] array = null;
                do
                {
                    num = RestartMgr.RmGetList(_sessionId, out var pnProcInfoNeeded, ref pnProcInfo, array, out var _);
                    switch (num)
                    {
                        case 234:
                            pnProcInfo = pnProcInfoNeeded;
                            array = new RestartMgr.RmProcessInfo[pnProcInfo];
                            continue;
                        default:
                            ThrowOnError(num);
                            continue;
                        case 0:
                            break;
                    }
                    break;
                }
                while (num == 234);
                if (array != null && array.Length != 0)
                    return array.Select(process => new LockingProcessInfo(process)).ToArray();
            }

            return Enumerable.Empty<LockingProcessInfo>();
        }

        internal static void ThrowOnError(int result)
        {
            if (result != 0)
                throw new WindowsException(result);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            var result = RestartMgr.RmEndSession(_sessionId);
            IsDisposed = true;
            ThrowOnError(result);
        }
    }
}