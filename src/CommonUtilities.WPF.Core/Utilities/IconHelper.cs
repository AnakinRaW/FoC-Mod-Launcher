using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

internal static class IconHelper
{
    private static BitmapSource? _smallIcon;
    private static BitmapSource? _largeIcon;
    private static BitmapSource? _windowIcon;
    private static bool _triedGettingIconsFromExecutable;
    private static bool _isWindowIconRetrieved;
    private static readonly object SyncLock = new();

    public static async Task UseWindowIconAsync(Action<ImageSource?> callback)
    {
        if (_isWindowIconRetrieved)
        {
            callback(_windowIcon);
        }
        else
        {
            if (!_triedGettingIconsFromExecutable)
                await Task.Run(() => GetWindowIcon(() => ExtractIconsFromExecutable(ref _smallIcon, ref _largeIcon), ref _triedGettingIconsFromExecutable));
            GetWindowIcon(() => ChooseOrEncodeWindowIcon(_smallIcon, _largeIcon), ref _isWindowIconRetrieved);
            callback(_windowIcon);
        }
    }

    private static void GetWindowIcon(Func<BitmapSource?> imageGetter, ref bool imageGotFlag)
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

    private static BitmapSource? ExtractIconsFromExecutable(ref BitmapSource? smallIcon, ref BitmapSource? largeIcon)
    {
        var executablePath = Assembly.GetEntryAssembly().Location;
        IntPtr[] phiconLarge = { IntPtr.Zero };
        IntPtr[] phiconSmall = { IntPtr.Zero };
        if (Shell32.ExtractIconEx(executablePath.Trim('"'), 0, phiconLarge, phiconSmall, 1) > 0)
        {
            smallIcon = BitmapSourceFromHIcon(phiconSmall[0]);
            largeIcon = BitmapSourceFromHIcon(phiconLarge[0]);
        }
        return null;
    }

    private static BitmapSource? BitmapSourceFromHIcon(IntPtr iconHandle)
    {
        BitmapSource? image = null;
        if (iconHandle != IntPtr.Zero)
        {
            image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            User32.DestroyIcon(iconHandle);
            FreezeImage(image);
        }
        return image;
    }

    private static void FreezeImage(ImageSource? image)
    {
        if (image == null || image.IsFrozen || !image.CanFreeze)
            return;
        image.Freeze();
    }

    private static BitmapSource? ChooseOrEncodeWindowIcon(BitmapSource? smallIcon, BitmapSource? largeIcon)
    {
        BitmapSource? bitmapSource = null;
        if (largeIcon != null)
        {
            if (smallIcon != null)
            {
                BitmapFrame? image;
                var tiffBitmapEncoder = new TiffBitmapEncoder();
                tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(smallIcon));
                tiffBitmapEncoder.Frames.Add(BitmapFrame.Create(largeIcon));
                using (var bitmapStream = new MemoryStream())
                {
                    tiffBitmapEncoder.Save(bitmapStream);
                    image = BitmapFrame.Create(bitmapStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                FreezeImage(image);
                bitmapSource = image;
            }
            else
                bitmapSource = largeIcon;
        }
        else if (smallIcon != null)
            bitmapSource = smallIcon;
        return bitmapSource;
    }
}