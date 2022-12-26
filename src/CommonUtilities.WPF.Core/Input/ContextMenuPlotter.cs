using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.DPI;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Sklavenwalker.CommonUtilities.Wpf.Utils;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

public sealed class ContextMenuPlotter : IContextMenuPlotter
{
    public static IContextMenuPlotter Instance => _instance ??= new ContextMenuPlotter();

    private Window? _currentMainWindow;

    private Window? _contextMenuPlacementWindow;

    private static IContextMenuPlotter? _instance;

    private Window ContextMenuPlacementWindow
    {
        get
        {
            if (_contextMenuPlacementWindow is null)
            {
                if (_currentMainWindow is null)
                {
                    var mainWindow = Application.Current.MainWindow ??
                                     throw new InvalidOperationException("The application does not have a main window set");
                    mainWindow.Closed += OnMainWindowClosed;
                    _currentMainWindow = mainWindow;
                }
                // There could be a race between registering the close event and the window really was closed.
                // I guess it's super rare for the needs of this class. We ignore this here.
                using (DpiHelper.EnterDpiScope(DpiHelper.ProcessDpiAwarenessContext))
                {
                    _contextMenuPlacementWindow = new PlacementTargetWindow();
                    DisplayHelper.SetWindowRect(_contextMenuPlacementWindow, Int32Rect.Empty);
                    _contextMenuPlacementWindow.Show();
                }
            }
            return _contextMenuPlacementWindow;
        }
    }

    private ContextMenuPlotter()
    {
    }

    public bool ShowContextMenu(ContextMenu contextMenu, UIElement? placementTarget)
    {
        var position = GetContextMenuLocation(ref placementTarget);
        return ShowContextMenu(contextMenu, position, placementTarget);
    }


    public bool ShowContextMenu(ContextMenu contextMenu, Point position, UIElement? placementTarget)
    {
        if (contextMenu is null)
            return false;
        UpdateContextMenuProperties(contextMenu, position);
        UtilityMethods.InvalidateRecursive(contextMenu);
        return true;
    }

    private void UpdateContextMenuPlacementPosition(Point absolutePoint)
    {
        User32.SetWindowPos(new WindowInteropHelper(ContextMenuPlacementWindow).Handle, IntPtr.Zero,
            (int)absolutePoint.X, (int)absolutePoint.Y, 0, 0, 17);
    }

    private void UpdateContextMenuProperties(ContextMenu contextMenu, Point point)
    {
        UpdateContextMenuPlacementPosition(point);
        using (DpiHelper.EnterDpiScope(DpiHelper.ProcessDpiAwarenessContext))
        {
            contextMenu.PlacementRectangle = new Rect(0.0, 0.0, 0.0, 0.0);
            contextMenu.PlacementTarget = ContextMenuPlacementWindow;
            contextMenu.IsOpen = true;
        }
    }

    private static Point GetContextMenuLocation(ref UIElement? uiElement)
    {
        var p = new Point();
        if (InputManager.Current.MostRecentInputDevice is KeyboardDevice)
        {
            if (uiElement is null)
            {
                if (Keyboard.FocusedElement is UIElement focusedElement)
                    p = focusedElement.PointToScreen(new Point(0, focusedElement.RenderSize.Height));
            }
            else
                p = uiElement.PointToScreen(new Point(0, uiElement.RenderSize.Height));
        }
        else
        {
            var messagePos = User32.GetMessagePos();
            p = new Point(User32.LoWord(messagePos), User32.HiWord(messagePos));
        }

        return uiElement?.DeviceToLogicalPoint(p) ?? DpiHelper.PointLogicalPoint(p);
    }

    private void OnMainWindowClosed(object sender, EventArgs e)
    {
        _currentMainWindow!.Closed -= OnMainWindowClosed;
        _currentMainWindow = null;
        _contextMenuPlacementWindow?.Close();
        _contextMenuPlacementWindow = null;
    }

    private class PlacementTargetWindow : Window
    {
        public PlacementTargetWindow()
        {
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowActivated = false;
            ShowInTaskbar = false;
            Visibility = Visibility.Hidden;
            WindowStyle = WindowStyle.None;
            Width = 0.0;
            Height = 0.0;
        }
    }
}