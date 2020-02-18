using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FocLauncherHost.Updater.NativeMethods
{
    internal static class RestartMgr
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RmProcessInfo
        {
            internal RmUniqueProcess Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            internal string strServiceShortName;
            internal ApplicationType ApplicationType;
            internal ApplicationStatus AppStatus;
            internal int TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            internal bool bRestartable;
        }

        internal struct RmUniqueProcess
        {
            public int DwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        internal delegate void RmWriteStatusCallback([MarshalAs(UnmanagedType.U4)] int nPercentComplete);

        [Flags]
        public enum WindowsRestartManagerShutdown
        {
            ForceShutdown = 1,
            ShutdownOnlyRegistered = 16,
        }

        public enum ApplicationType
        {
            Unknown = 0,
            MainWindow = 1,
            OtherWindow = 2,
            Service = 3,
            Explorer = 4,
            Console = 5,
            Critical = 1000
        }

        [Flags]
        public enum ApplicationStatus
        {
            Unknown = 0,
            Running = 1,
            Stopped = 2,
            StoppedOther = 4,
            Restarted = 8,
            ErrorOnStop = 16,
            ErrorOnRestart = 32,
            ShutdownMasked = 64,
            RestartMasked = 128,
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmStartSession(out int pSessionHandle, int dwSessionFlags, [Out] StringBuilder strSessionKey);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmRegisterResources(int dwSessionHandle, int nFiles, string[] fileNames, 
            int nApplications, RmUniqueProcess[] rgApplications, int nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmGetList(int dwSessionHandle, out int pnProcInfoNeeded, ref int pnProcInfo, [Out] RmProcessInfo[] rgAffectedApps, out int lpdwRebootReasons);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmShutdown(int dwSessionHandle, WindowsRestartManagerShutdown lActionFlags, RmWriteStatusCallback fnStatus);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmRestart(int dwSessionHandle, int dwRestartFlags, RmWriteStatusCallback fnStatus);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmEndSession(int dwSessionHandle);
    }
}