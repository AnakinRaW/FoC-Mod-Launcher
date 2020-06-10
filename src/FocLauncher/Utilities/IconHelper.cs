using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FocLauncher.NativeMethods;

namespace FocLauncher.Utilities
{
    internal static class IconHelper
    {
        private static bool _triedGettingIconsFromExecutable;
        private static bool _isWindowIconRetrieved;
        private static BitmapSource _windowIcon;
        private static BitmapSource _largeIcon;
        private static BitmapSource _smallIcon;
        private static readonly object SyncLock = new object();

        public static async Task UseWindowIconAsync(Action<ImageSource> callback)
        {
            if (_isWindowIconRetrieved) 
                callback(_windowIcon);
            else
            {
                if (!_triedGettingIconsFromExecutable)
                    await Task.Run(() => GetWindowIcon(() => ExtractIconsFromExecutable(ref _smallIcon, ref _largeIcon), ref _triedGettingIconsFromExecutable));
                GetWindowIcon(() => ChooseOrEncodeWindowIcon(_smallIcon, _largeIcon), ref _isWindowIconRetrieved);
                callback(_windowIcon);
            }
        }

        private static BitmapSource ExtractIconsFromExecutable(ref BitmapSource smallIcon, ref BitmapSource largeIcon)
        {
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var phiconLarge = new IntPtr[1]
            {
                IntPtr.Zero
            };
            var phiconSmall = new IntPtr[1]
            {
                IntPtr.Zero
            };
            if (Shell32.ExtractIconEx(executablePath.Trim('"'), 0, phiconLarge, phiconSmall, 1) > 0)
            {
                smallIcon = BitmapSourceFromHIcon(phiconSmall[0]);
                largeIcon = BitmapSourceFromHIcon(phiconLarge[0]);
            }
            return null;
        }

        private static BitmapSource BitmapSourceFromHIcon(IntPtr iconHandle)
        {
            BitmapSource bitmapSource = null;
            if (iconHandle != IntPtr.Zero)
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                User32.DestroyIcon(iconHandle);
                FreezeImage(bitmapSource);
            }
            return bitmapSource;
        }

        private static void GetWindowIcon(Func<BitmapSource> imageGetter, ref bool imageGotFlag)
        {
            lock (SyncLock)
            {
                if (imageGotFlag)
                    return;
                try
                {
                    _windowIcon = imageGetter();
                }
                finally
                {
                    imageGotFlag = true;
                }
            }
        }

        private static void FreezeImage(ImageSource image)
        {
            if (image == null || image.IsFrozen || !image.CanFreeze)
                return;
            image.Freeze();
        }

        private static BitmapSource ChooseOrEncodeWindowIcon(BitmapSource smallIcon, BitmapSource largeIcon)
        {
            BitmapSource bitmapSource = null;
            if (largeIcon != null)
            {
                if (smallIcon != null)
                {
                    BitmapFrame bitmapFrame;
                    var tiffBitmapEncoder = new TiffBitmapEncoder();
                    tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(smallIcon));
                    tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(largeIcon));
                    using (var memoryStream = new MemoryStream())
                    {
                        tiffBitmapEncoder.Save(memoryStream);
                        bitmapFrame = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                    FreezeImage(bitmapFrame);
                    bitmapSource = bitmapFrame;
                }
                else
                    bitmapSource = largeIcon;
            }
            else if (smallIcon != null)
                bitmapSource = smallIcon;
            return bitmapSource;
        }
    }
}