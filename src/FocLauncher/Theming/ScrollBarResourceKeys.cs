using System.Windows;

namespace FocLauncher.Theming
{
    internal static class ScrollBarResourceKeys
    {
        private static object? _scrollBarStyleKey;
        private static object? _scrollViewerStyleKey;
        private static object? _customGridViewScrollViewerStyleKey;

        public static object UnthemedScrollBarStyleKey => "ResourceKeys.UnthemedScrollBarStyleKey";

        public static object UnthemedGridViewScrollViewerStyleKey => "ResourceKeys.UnthemedGridViewScrollViewerStyleKey";

        public static object UnthemedScrollViewerStyleKey => "ResourceKeys.UnthemedScrollViewerStyleKey";

        public static object ScrollViewerStyleKey =>
            _scrollViewerStyleKey ??= GetResourceKey(nameof(ScrollViewerStyleKey));

        public static object ScrollBarStyleKey => _scrollBarStyleKey ??= GetResourceKey(nameof(ScrollBarStyleKey));

        public static object CustomGridViewScrollViewerStyleKey => _customGridViewScrollViewerStyleKey ??=
            GetResourceKey(nameof(CustomGridViewScrollViewerStyleKey));

        public static object GetScrollBarStyleKey(bool themed)
        {
            return !themed ? UnthemedScrollBarStyleKey : ScrollBarStyleKey;
        }

        public static object GetScrollViewerStyleKey(bool themed)
        {
            return !themed ? UnthemedScrollViewerStyleKey : ScrollViewerStyleKey;
        }

        public static object GetGridViewScrollViewerStyleKey(bool themed)
        {
            return !themed ? UnthemedGridViewScrollViewerStyleKey : CustomGridViewScrollViewerStyleKey;
        }

        private static object GetResourceKey(string resourceId)
        {
            return new ComponentResourceKey(typeof(MainWindow), resourceId);
        }
    }
}