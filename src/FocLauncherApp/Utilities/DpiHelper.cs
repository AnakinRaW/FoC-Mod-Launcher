using System;
using System.Windows;
using System.Windows.Media;

namespace FocLauncherApp.Utilities
{
    internal static class DpiHelper
    {
        private static DpiHelperImpl Instance { get; }

        static DpiHelper()
        {
            Instance = new DpiHelperImpl(96.0);
        }

        public static Rect DeviceToLogicalUnits(this Rect deviceRect)
        {
            return Instance.DeviceToLogicalUnits(deviceRect);
        }

        private class DpiHelperImpl
        {
            private MatrixTransform TransformFromDevice { get; }

            private MatrixTransform TransformToDevice { get; }

            private double DeviceDpiX { get; }

            private double DeviceDpiY { get; }

            private double LogicalDpiX { get; }

            private double LogicalDpiY { get; }

            public DpiHelperImpl(double logicalDpi)
            {
                LogicalDpiX = logicalDpi;
                LogicalDpiY = logicalDpi;
                var dc = NativeMethods.NativeMethods.GetDC(IntPtr.Zero);
                if (dc != IntPtr.Zero)
                {
                    DeviceDpiX = NativeMethods.NativeMethods.GetDeviceCaps(dc, 88);
                    DeviceDpiY = NativeMethods.NativeMethods.GetDeviceCaps(dc, 90);
                    NativeMethods.NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                }
                else
                {
                    DeviceDpiX = LogicalDpiX;
                    DeviceDpiY = LogicalDpiY;
                }
                var identity1 = Matrix.Identity;
                var identity2 = Matrix.Identity;
                identity1.Scale(DeviceDpiX / LogicalDpiX, DeviceDpiY / LogicalDpiY);
                identity2.Scale(LogicalDpiX / DeviceDpiX, LogicalDpiY / DeviceDpiY);
                TransformFromDevice = new MatrixTransform(identity2);
                TransformFromDevice.Freeze();
                TransformToDevice = new MatrixTransform(identity1);
                TransformToDevice.Freeze();
            }

            public Rect DeviceToLogicalUnits(Rect deviceRect)
            {
                var rect = deviceRect;
                rect.Transform(TransformFromDevice.Matrix);
                return rect;
            }
        }
    }
}