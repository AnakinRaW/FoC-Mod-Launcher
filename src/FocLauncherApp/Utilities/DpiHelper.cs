using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Size = System.Windows.Size;

namespace FocLauncherApp.Utilities
{
    internal static class DpiHelper
    {
        public static DpiHelperImplementation Instance { get; }

        public static ImageScalingMode ImageScalingMode => Instance.ImageScalingMode;

        public static BitmapScalingMode BitmapScalingMode => Instance.BitmapScalingMode;

        public static bool UsePreScaledImages => Instance.UsePreScaledImages;

        public static MatrixTransform TransformFromDevice => Instance.TransformFromDevice;

        public static MatrixTransform TransformToDevice => Instance.TransformToDevice;

        public static double DeviceDpiX => Instance.DeviceDpiX;

        public static double DeviceDpiY => Instance.DeviceDpiY;

        public static double LogicalDpiX => Instance.LogicalDpiX;

        public static double LogicalDpiY => Instance.LogicalDpiY;

        public static bool IsScalingRequired => Instance.IsScalingRequired;

        public static double DeviceToLogicalUnitsScalingFactorX => Instance.DeviceToLogicalUnitsScalingFactorX;

        public static double DeviceToLogicalUnitsScalingFactorY => Instance.DeviceToLogicalUnitsScalingFactorY;

        public static double LogicalToDeviceUnitsScalingFactorX => Instance.LogicalToDeviceUnitsScalingFactorX;

        public static double LogicalToDeviceUnitsScalingFactorY => Instance.LogicalToDeviceUnitsScalingFactorY;

        public static int DpiScalePercentX => Instance.DpiScalePercentX;

        public static int DpiScalePercentY => Instance.DpiScalePercentY;

        public static double PreScaledImageLayoutTransformScaleX => Instance.PreScaledImageLayoutTransformScaleX;

        public static double PreScaledImageLayoutTransformScaleY => Instance.PreScaledImageLayoutTransformScaleY;

        static DpiHelper()
        {
            Instance = new DpiHelperImplementation();
        }

        public static double LogicalToDeviceUnitsX(double value)
        {
            return Instance.LogicalToDeviceUnitsX(value);
        }

        public static double LogicalToDeviceUnitsY(double value)
        {
            return Instance.LogicalToDeviceUnitsY(value);
        }

        public static double DeviceToLogicalUnitsX(double value)
        {
            return Instance.DeviceToLogicalUnitsX(value);
        }

        public static double DeviceToLogicalUnitsY(double value)
        {
            return Instance.DeviceToLogicalUnitsY(value);
        }

        public static float LogicalToDeviceUnitsX(float value)
        {
            return Instance.LogicalToDeviceUnitsX(value);
        }

        public static float LogicalToDeviceUnitsY(float value)
        {
            return Instance.LogicalToDeviceUnitsY(value);
        }

        public static int LogicalToDeviceUnitsX(int value)
        {
            return Instance.LogicalToDeviceUnitsX(value);
        }

        public static int LogicalToDeviceUnitsY(int value)
        {
            return Instance.LogicalToDeviceUnitsY(value);
        }

        public static float DeviceToLogicalUnitsX(float value)
        {
            return Instance.DeviceToLogicalUnitsX(value);
        }

        public static float DeviceToLogicalUnitsY(float value)
        {
            return Instance.DeviceToLogicalUnitsY(value);
        }

        public static int DeviceToLogicalUnitsX(int value)
        {
            return Instance.DeviceToLogicalUnitsX(value);
        }

        public static int DeviceToLogicalUnitsY(int value)
        {
            return Instance.DeviceToLogicalUnitsY(value);
        }

        public static double RoundToDeviceUnitsX(double value)
        {
            return Instance.RoundToDeviceUnitsX(value);
        }

        public static double RoundToDeviceUnitsY(double value)
        {
            return Instance.RoundToDeviceUnitsY(value);
        }

        public static System.Windows.Point LogicalToDeviceUnits(this System.Windows.Point logicalPoint)
        {
            return Instance.LogicalToDeviceUnits(logicalPoint);
        }

        public static Rect LogicalToDeviceUnits(this Rect logicalRect)
        {
            return Instance.LogicalToDeviceUnits(logicalRect);
        }

        public static Size LogicalToDeviceUnits(this Size logicalSize)
        {
            return Instance.LogicalToDeviceUnits(logicalSize);
        }

        public static Thickness LogicalToDeviceUnits(this Thickness logicalThickness)
        {
            return Instance.LogicalToDeviceUnits(logicalThickness);
        }

        public static System.Windows.Point DeviceToLogicalUnits(this System.Windows.Point devicePoint)
        {
            return Instance.DeviceToLogicalUnits(devicePoint);
        }

        public static Rect DeviceToLogicalUnits(this Rect deviceRect)
        {
            return Instance.DeviceToLogicalUnits(deviceRect);
        }

        public static Size DeviceToLogicalUnits(this Size deviceSize)
        {
            return Instance.DeviceToLogicalUnits(deviceSize);
        }

        public static Thickness DeviceToLogicalUnits(this Thickness deviceThickness)
        {
            return Instance.DeviceToLogicalUnits(deviceThickness);
        }

        public static void SetDeviceLeft(this Window window, double deviceLeft)
        {
            Instance.SetDeviceLeft(ref window, deviceLeft);
        }

        public static double GetDeviceLeft(this Window window)
        {
            return Instance.GetDeviceLeft(window);
        }

        public static void SetDeviceTop(this Window window, double deviceTop)
        {
            Instance.SetDeviceTop(ref window, deviceTop);
        }

        public static double GetDeviceTop(this Window window)
        {
            return Instance.GetDeviceTop(window);
        }

        public static void SetDeviceWidth(this Window window, double deviceWidth)
        {
            Instance.SetDeviceWidth(ref window, deviceWidth);
        }

        public static double GetDeviceWidth(this Window window)
        {
            return Instance.GetDeviceWidth(window);
        }

        public static void SetDeviceHeight(this Window window, double deviceHeight)
        {
            Instance.SetDeviceHeight(ref window, deviceHeight);
        }

        public static double GetDeviceHeight(this Window window)
        {
            return Instance.GetDeviceHeight(window);
        }

        public static Rect GetDeviceRect(this Window window)
        {
            return Instance.GetDeviceRect(window);
        }

        public static Size GetDeviceActualSize(this FrameworkElement element)
        {
            return Instance.GetDeviceActualSize(element);
        }

        public static System.Drawing.Point LogicalToDeviceUnits(this System.Drawing.Point logicalPoint)
        {
            return Instance.LogicalToDeviceUnits(logicalPoint);
        }

        public static System.Drawing.Size LogicalToDeviceUnits(this System.Drawing.Size logicalSize)
        {
            return Instance.LogicalToDeviceUnits(logicalSize);
        }

        public static Rectangle LogicalToDeviceUnits(this Rectangle logicalRect)
        {
            return Instance.LogicalToDeviceUnits(logicalRect);
        }

        public static PointF LogicalToDeviceUnits(this PointF logicalPoint)
        {
            return Instance.LogicalToDeviceUnits(logicalPoint);
        }

        public static SizeF LogicalToDeviceUnits(this SizeF logicalSize)
        {
            return Instance.LogicalToDeviceUnits(logicalSize);
        }

        public static RectangleF LogicalToDeviceUnits(this RectangleF logicalRect)
        {
            return Instance.LogicalToDeviceUnits(logicalRect);
        }

        public static void LogicalToDeviceUnits(ref Bitmap bitmapImage, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref bitmapImage, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Bitmap bitmapImage, System.Drawing.Color backgroundColor, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref bitmapImage, backgroundColor, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Image image, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref image, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Image image, System.Drawing.Color backgroundColor, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref image, backgroundColor, scalingMode);
        }

        public static Image CreateDeviceFromLogicalImage(this Image logicalImage, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            return Instance.CreateDeviceFromLogicalImage(logicalImage, scalingMode);
        }

        public static ImageSource ScaleLogicalImageForDeviceSize(ImageSource image, Size deviceImageSize, BitmapScalingMode scalingMode)
        {
            return Instance.ScaleLogicalImageForDeviceSize(image, deviceImageSize, scalingMode);
        }

        public static Image CreateDeviceFromLogicalImage(this Image logicalImage, System.Drawing.Color backgroundColor, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            return Instance.CreateDeviceFromLogicalImage(logicalImage, backgroundColor, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Bitmap imageStrip, System.Drawing.Size logicalImageSize, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref imageStrip, logicalImageSize, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Bitmap imageStrip, System.Drawing.Size logicalImageSize, System.Drawing.Color backgroundColor, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref imageStrip, logicalImageSize, backgroundColor, scalingMode);
        }

        public static Bitmap CreateDeviceFromLogicalImage(this Bitmap logicalBitmapStrip, System.Drawing.Size logicalImageSize, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            return Instance.CreateDeviceFromLogicalImage(logicalBitmapStrip, logicalImageSize, scalingMode);
        }

        public static Bitmap CreateDeviceFromLogicalImage(this Bitmap logicalBitmapStrip, System.Drawing.Size logicalImageSize, System.Drawing.Color backgroundColor, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            return Instance.CreateDeviceFromLogicalImage(logicalBitmapStrip, logicalImageSize, backgroundColor, scalingMode);
        }

        public static void LogicalToDeviceUnits(ref Icon icon, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            Instance.LogicalToDeviceUnits(ref icon, scalingMode);
        }

        public static Icon CreateDeviceFromLogicalImage(this Icon logicalIcon, ImageScalingMode scalingMode = ImageScalingMode.Default)
        {
            return Instance.CreateDeviceFromLogicalImage(logicalIcon, scalingMode);
        }

        public class DpiHelperImplementation : DpiHelperImpl
        {
            public DpiHelperImplementation()
                : base(96.0)
            {
            }
        }
    }
}