using System;
using System.Windows;
using System.Windows.Media;

namespace FocLauncherApp.Utilities
{
    public class DpiHelperImpl
    {
        protected const double DefaultLogicalDpi = 96.0;

        public MatrixTransform TransformFromDevice { get; }

        public MatrixTransform TransformToDevice { get; }

        public double DeviceDpiX { get; }

        public double DeviceDpiY { get; }

        public double LogicalDpiX { get; }

        public double LogicalDpiY { get; }

        protected DpiHelperImpl(double logicalDpi)
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