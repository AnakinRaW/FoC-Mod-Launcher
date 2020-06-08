using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FocLauncher.Dialogs;
using FocLauncher.Mods;
using FocLauncher.NativeMethods;
using FocLauncher.Theming;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher
{
    public partial class MainWindow
    {
        public static RoutedCommand MinimizeWindow = new RoutedCommand(nameof(MinimizeWindow), typeof(MainWindow));
        public static RoutedCommand CloseWindow = new RoutedCommand(nameof(CloseWindow), typeof(MainWindow));


        static MainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(MainWindow)));
            RuntimeHelpers.RunClassConstructor(typeof(ScrollBarThemingUtilities).TypeHandle);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(MinimizeWindow, OnMinimizeWindow));
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(CloseWindow, OnCloseWindow));
        }

        public MainWindow()
        {
            Application.Current.Resources.Add(LauncherFonts.EaWBoldFontFamilyKey, 
                new FontFamily(new Uri("pack://application:,,,/FocLauncher.Core;component/Resources/Fonts/", UriKind.Absolute), 
                    "./#Empire At War Bold"));

            InitializeComponent();
            ScrollBarThemingUtilities.SetThemeScrollBars(this, true);
            ListBox.Focus();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetWindowIcon();
        }

        private void SetWindowIcon()
        {
            IconHelper.UseWindowIconAsync(windowIcon =>
            {
                Dispatcher.Invoke(new Action(() => Icon = windowIcon));
            }).Forget();
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow(this).ShowDialog();
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).ShowDialog();
        }

        private void OpenChangeThemeDialog(object sender, RoutedEventArgs e)
        {
            new ChangeThemeDialog(this).ShowDialog();
        }
        
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SteamModNamePersister.Instance.Save();
        }

        private static bool CanMinimizeWindow(ExecutedRoutedEventArgs args)
        {
            return args.Parameter is Window;
        }

        private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs args)
        {
            if (!CanMinimizeWindow(args))
                return;
            ((Window)args.Parameter).WindowState = WindowState.Minimized;
        }

        private static bool CanCloseWindow(ExecutedRoutedEventArgs args)
        {
            return args.Parameter is Window;
        }

        private static void OnCloseWindow(object sender, ExecutedRoutedEventArgs args)
        {
            if (!CanCloseWindow(args))
                return;
            ((Window)args.Parameter).Close();
        }
    }

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
