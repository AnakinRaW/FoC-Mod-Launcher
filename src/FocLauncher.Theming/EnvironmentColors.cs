using System.Windows;

namespace FocLauncher.Theming
{
    public static class EnvironmentColors
    {
        private static ComponentResourceKey _backgroundColor;
        private static ComponentResourceKey _backgroundImage;

        private static ComponentResourceKey _statusBarDefaultBackground;
        private static ComponentResourceKey _statusBarDefaultText;
        public static ComponentResourceKey BackgroundColor =>
            _backgroundColor ?? (_backgroundColor = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundColor)));

        public static ComponentResourceKey BackgroundImage =>
            _backgroundImage ?? (_backgroundImage = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundImage)));


        public static ComponentResourceKey StatusBarDefaultBackground =>
            _statusBarDefaultBackground ?? (_statusBarDefaultBackground = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultBackground)));

        public static ComponentResourceKey StatusBarDefaultText =>
            _statusBarDefaultText ?? (_statusBarDefaultText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultText)));
    }
}
