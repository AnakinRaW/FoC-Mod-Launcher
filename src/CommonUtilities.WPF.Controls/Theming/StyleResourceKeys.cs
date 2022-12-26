using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming
{
    public static class StyleResourceKeys
    {
        private static object? _menuItemStyleKey;
        private static object? _scrollBarStyleKey;
        private static object? _scrollViewerStyleKey;
        private static object? _customGridViewScrollViewerStyleKey;

        public static object MenuItemStyleKey => _menuItemStyleKey ??= GetResourceKey(nameof(MenuItemStyleKey));

        public static object UnthemedScrollBarStyleKey => "DummyUnthemedScrollBarStyleKey";
        public static object UnthemedScrollViewerStyleKey => "DummyUnthemedScrollViewerStyleKey";

        public static object ScrollViewerStyleKey => _scrollViewerStyleKey ??= GetResourceKey(nameof(ScrollViewerStyleKey));
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

        private static object GetResourceKey(object resourceId)
        {
            return new ComponentResourceKey(typeof(WindowBase), resourceId);
        }
    }
}
