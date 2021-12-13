using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming
{
    public static class StyleResourceKeys
    {
        private static object? _menuItemStyleKey;

        public static object? MenuItemStyleKey => _menuItemStyleKey ??= GetResourceKey(nameof(MenuItemStyleKey));

        private static object GetResourceKey(object resourceId)
        {
            return new ComponentResourceKey(typeof(ThemedWindow), resourceId);
        }
    }
}
