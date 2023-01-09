using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Themes
{
    public static class StyleResourceKeys
    {
        public static object UnthemedScrollBarStyleKey => "DummyUnthemedScrollBarStyleKey";
        public static object UnthemedScrollViewerStyleKey => "DummyUnthemedScrollViewerStyleKey";

        public static object MenuItemStyleKey { get; } = GetResourceKey(nameof(MenuItemStyleKey));
        public static object ThemedComboBoxStyleKey { get; } = GetResourceKey(nameof(ThemedComboBoxStyleKey));
        public static object ScrollViewerStyleKey { get; } = GetResourceKey(nameof(ScrollViewerStyleKey));
        public static object ScrollBarStyleKey { get; } = GetResourceKey(nameof(ScrollBarStyleKey));
        public static object CustomGridViewScrollViewerStyleKey { get; } = GetResourceKey(nameof(CustomGridViewScrollViewerStyleKey));

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
            return new ComponentResourceKey(typeof(StyleResourceKeys), resourceId);
        }
    }
}
