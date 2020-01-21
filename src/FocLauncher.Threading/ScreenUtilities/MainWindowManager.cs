using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using FocLauncher.NativeMethods;

namespace FocLauncher.ScreenUtilities
{
    internal class MainWindowManager : INotifyPropertyChanged
    {
        private HwndSource _mainWindowHwndSource;
        private int _display;
        private static MainWindowManager _instance;
        internal ContentControl MainWindowContent { get; private set; }

        public int Display
        {
            get => _display;
            set
            {
                if (value == _display) return;
                _display = value;
                OnPropertyChanged();
            }
        }

        private HwndSource MainWindowHwndSource
        {
            set
            {
                if (_mainWindowHwndSource == value)
                    return;
                _mainWindowHwndSource = value;
                BroadcastMessageMonitor.Instance.HwndSource = _mainWindowHwndSource;
            }
        }

        public bool IsInitialized => MainWindowContent != null;

        public static MainWindowManager Instance
        {
            get => _instance ??= new MainWindowManager();
            internal set => _instance = value;
        }
        
        public void Initialize(ContentControl mainWindowContent)
        {
            MainWindowContent = mainWindowContent ?? throw new ArgumentNullException(nameof(mainWindowContent));
            InitializePresentationSource(mainWindowContent);
        }

        private void InitializePresentationSource(UIElement content)
        {
            PresentationSource.AddSourceChangedHandler(content, OnPresentationSourceChanged);
            UpdateMainWindowHandle(content);
        }

        private void UpdateMainWindowHandle(UIElement content)
        {
            MainWindowHwndSource = FindTopLevelHwndSource(content);
        }

        private void OnPresentationSourceChanged(object sender, SourceChangedEventArgs args)
        {
            UpdateMainWindowHandle(MainWindowContent);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal static HwndSource FindTopLevelHwndSource(UIElement element)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(element);
            if (hwndSource != null && IsChildWindow(hwndSource.Handle))
                hwndSource = HwndSource.FromHwnd(FindTopLevelWindow(hwndSource.Handle));
            return hwndSource;
        }

        internal static IntPtr FindTopLevelWindow(IntPtr hWnd)
        {
            while (hWnd != IntPtr.Zero && IsChildWindow(hWnd))
                hWnd = NativeMethods.NativeMethods.GetParent(hWnd);
            return hWnd;
        }

        internal static bool IsChildWindow(IntPtr hWnd)
        {
            return (NativeMethods.NativeMethods.GetWindowLong(hWnd, (int)Gwl.Style) & 1073741824) == 1073741824;
        }
    }
}