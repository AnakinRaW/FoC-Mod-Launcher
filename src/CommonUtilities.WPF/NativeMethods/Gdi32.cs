﻿using System;
using System.Runtime.InteropServices;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal static class Gdi32
{
    [DllImport("gdi32.dll")]
    internal static extern int GetDeviceCaps(IntPtr hdc, int index);
}