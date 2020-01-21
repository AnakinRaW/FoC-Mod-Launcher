using System;

namespace FocLauncher.WaitDialog
{
    [Serializable]
    public class DialogInitializationArguments
    {
        public string AppName { get; set; }

        public IntPtr AppMainWindowHandle { get; set; }
        
        public uint BackgroundColor { get; set; }

        public uint TextColor { get; set; }

        public uint CaptionTextColor { get; set; }

        public uint CaptionBackgroundColor { get; set; }

        public uint BorderColor { get; set; }

        public int AppProcessId { get; set; }

        public string CancelText { get; set; }
    }
}