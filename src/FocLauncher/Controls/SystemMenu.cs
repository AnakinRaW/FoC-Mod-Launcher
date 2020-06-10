using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FocLauncher.NativeMethods;
using FocLauncher.ScreenUtilities;
using FocLauncher.Utilities;

namespace FocLauncher.Controls
{
    public sealed class SystemMenu : Control, INonClientArea
    {
        public static readonly DependencyProperty SourceProperty = Image.SourceProperty.AddOwner(typeof(SystemMenu), new FrameworkPropertyMetadata(OnSourceChanged));
        private ImageSource _optimalImageForSize;

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        static SystemMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SystemMenu), new FrameworkPropertyMetadata(typeof(SystemMenu)));
        }
        

        int INonClientArea.HitTest(Point point)
        {
            return 1;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = new Size(Math.Max(0.0, RenderSize.Width - Padding.Left - Padding.Right), Math.Max(0.0, RenderSize.Height - Padding.Top - Padding.Bottom));
            CoerceOptimalImageForSize(size);
            if (_optimalImageForSize == null)
                return;
            drawingContext.DrawImage(_optimalImageForSize, new Rect(new Point(Padding.Left, Padding.Top), size));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var handle = new WindowInteropHelper(this.FindAncestor<Window>()).Handle;
                var point = new Point(PointToScreen(new Point()).X, PointToScreen(new Point(0.0, ActualHeight)).Y);
                User32.SendMessage(handle, 787, IntPtr.Zero, NativeMethods.NativeMethods.MakeParam((int)point.X, (int)point.Y));
            }
            else
            {
                if (e.ClickCount != 2)
                    return;
                var ancestor = this.FindAncestor<Window>();
                MainWindow.CloseWindow.Execute(ancestor, this);
            }
            e.Handled = true;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (e.ClickCount != 1)
                return;
            var handle = new WindowInteropHelper(this.FindAncestor<Window>()).Handle;
            var screen = PointToScreen(e.MouseDevice.GetPosition(this));
            User32.SendMessage(handle, 787, IntPtr.Zero, NativeMethods.NativeMethods.MakeParam((int)screen.X, (int)screen.Y));
            e.Handled = true;
        }

        private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((SystemMenu)o).OnSourceChanged();
        }

        private void CoerceOptimalImageForSize(Size targetSize)
        {
            if (_optimalImageForSize != null)
                return;
            var source = Source as BitmapFrame;
            var deviceUnitsX = (int)this.LogicalToDeviceUnitsX(targetSize.Width);
            var num = -1;
            if (source != null)
            {
                foreach (var frame in source.Decoder.Frames)
                {
                    var pixelWidth = frame.PixelWidth;
                    if (pixelWidth == deviceUnitsX)
                    {
                        _optimalImageForSize = frame;
                        break;
                    }
                    if (pixelWidth > deviceUnitsX)
                    {
                        if (num < deviceUnitsX || pixelWidth < num)
                        {
                            num = pixelWidth;
                            _optimalImageForSize = frame;
                        }
                    }
                    else if (pixelWidth > num)
                    {
                        num = pixelWidth;
                        _optimalImageForSize = frame;
                    }
                }
            }
            else
                _optimalImageForSize = Source;
        }

        private void OnSourceChanged()
        {
            _optimalImageForSize = null;
            InvalidateVisual();
        }
    }
}
