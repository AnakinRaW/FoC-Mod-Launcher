using System;
using System.Runtime.InteropServices;

namespace FocLauncher.AppUpdater.NativeMethods
{
    internal class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);


        [Flags]
        internal enum MoveFileFlags
        {
            None = 0,
            MoveFileReplaceExisting = 1,
            MoveFileCopyAllowed = 2,
            MoveFileDelayUntilReboot = 4,
            MoveFileWriteThrough = 8,
        }
    }
}