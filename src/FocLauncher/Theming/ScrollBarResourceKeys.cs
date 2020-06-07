using System.Windows;

namespace FocLauncher.Theming
{
    internal static class ScrollBarResourceKeys
    {
        private static object _scrollBarStyleKey;
        private static object _scrollViewerStyleKey;
        private static object _customGridViewScrollViewerStyleKey;

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
            if (!themed)
                return UnthemedScrollBarStyleKey;
            return ScrollBarStyleKey;
        }

        public static object GetScrollViewerStyleKey(bool themed)
        {
            if (!themed)
                return UnthemedScrollViewerStyleKey;
            return ScrollViewerStyleKey;
        }

        public static object GetGridViewScrollViewerStyleKey(bool themed)
        {
            if (!themed)
                return UnthemedGridViewScrollViewerStyleKey;
            return CustomGridViewScrollViewerStyleKey;
        }

        private static object GetResourceKey(string resourceId)
        {
            return new ComponentResourceKey(typeof(MainWindow), resourceId);
        }
    }
}