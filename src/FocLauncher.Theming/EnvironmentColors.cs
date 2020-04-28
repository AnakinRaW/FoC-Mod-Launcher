using System.Windows;

namespace FocLauncher.Theming
{
    public static class EnvironmentColors
    {
        private static ComponentResourceKey _backgroundColor;
        private static ComponentResourceKey _backgroundImage;

        private static ComponentResourceKey _captionText;
        private static ComponentResourceKey _windowText;
        private static ComponentResourceKey _checkBoxText;

        private static ComponentResourceKey _statusBarDefaultBackground;
        private static ComponentResourceKey _statusBarDefaultText;

        private static ComponentResourceKey _statusBarRunningBackground;
        private static ComponentResourceKey _statusBarRunningText;

        private static ComponentResourceKey _waitWindowBackground;
        private static ComponentResourceKey _waitWindowBorder;
        private static ComponentResourceKey _waitWindowText;
        private static ComponentResourceKey _waitWindowCaptionBackground;
        private static ComponentResourceKey _waitWindowCaptionText;
        
        public static ComponentResourceKey BackgroundColor =>
            _backgroundColor ?? (_backgroundColor = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundColor)));

        public static ComponentResourceKey BackgroundImage =>
            _backgroundImage ?? (_backgroundImage = new ComponentResourceKey(typeof(EnvironmentColors), nameof(BackgroundImage)));

        public static ComponentResourceKey CaptionText =>
            _captionText ?? (_captionText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(CaptionText)));

        public static ComponentResourceKey WindowText =>
            _windowText ?? (_windowText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WindowText)));

        public static ComponentResourceKey CheckBoxText =>
            _checkBoxText ?? (_checkBoxText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(CheckBoxText)));


        public static ComponentResourceKey StatusBarDefaultBackground =>
            _statusBarDefaultBackground ?? (_statusBarDefaultBackground = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultBackground)));

        public static ComponentResourceKey StatusBarDefaultText =>
            _statusBarDefaultText ?? (_statusBarDefaultText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarDefaultText)));

        public static ComponentResourceKey StatusBarRunningBackground =>
            _statusBarRunningBackground ?? (_statusBarRunningBackground = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarRunningBackground)));

        public static ComponentResourceKey StatusBarRunningText =>
            _statusBarRunningText ?? (_statusBarRunningText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(StatusBarRunningText)));


        public static ComponentResourceKey WaitWindowBackground =>
            _waitWindowBackground ?? (_waitWindowBackground = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowBackground)));

        public static ComponentResourceKey WaitWindowBorder =>
            _waitWindowBorder ?? (_waitWindowBorder = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowBorder)));

        public static ComponentResourceKey WaitWindowText =>
            _waitWindowText ?? (_waitWindowText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowText)));

        public static ComponentResourceKey WaitWindowCaptionBackground =>
            _waitWindowCaptionBackground ?? (_waitWindowCaptionBackground = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowCaptionBackground)));

        public static ComponentResourceKey WaitWindowCaptionText =>
            _waitWindowCaptionText ?? (_waitWindowCaptionText = new ComponentResourceKey(typeof(EnvironmentColors), nameof(WaitWindowCaptionText)));
    }
}
