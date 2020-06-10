using System;
using System.Windows;
using System.Windows.Media;

namespace FocLauncher
{
    internal static class ApplicationResourceLoader
    {
        public static void LoadResources()
        {
            Application.Current.Resources.Add(LauncherFonts.EaWBoldFontFamilyKey,
                new FontFamily(new Uri("pack://application:,,,/FocLauncher.Core;component/Resources/Fonts/", UriKind.Absolute),
                    "./#Empire At War Bold"));
        }
    }
}