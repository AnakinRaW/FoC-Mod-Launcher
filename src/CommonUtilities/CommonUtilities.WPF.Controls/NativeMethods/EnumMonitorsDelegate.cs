using System;
using System.Runtime.InteropServices;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

[return: MarshalAs(UnmanagedType.Bool)]
internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RectStruct lprcMonitor, IntPtr dwData);