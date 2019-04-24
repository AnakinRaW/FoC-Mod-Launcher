using System.Windows;

namespace FocLauncher.Theming
{
    public static class EnvironmentColors
    {
        private static ComponentResourceKey _backgroundColor;
        private static ComponentResourceKey _backgroundImage;

        public static ComponentResourceKey BackgroundColor =>
            _backgroundColor ?? (_backgroundColor = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundColor)));

        public static ComponentResourceKey BackgroundImage =>
            _backgroundImage ?? (_backgroundImage = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundImage)));
    }
}
