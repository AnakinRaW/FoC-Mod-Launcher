using System;

namespace FocLauncherApp.Utilities
{
    public static class UnsafeHelpers
    {
        public static unsafe bool IsOptionalOutParamSet(out int param)
        {
            fixed (int* numPtr = &param)
                return (IntPtr)numPtr != IntPtr.Zero;
        }

        public static unsafe bool IsOptionalOutParamSet(out bool param)
        {
            fixed (bool* flagPtr = &param)
                return (IntPtr)flagPtr != IntPtr.Zero;
        }
    }
}