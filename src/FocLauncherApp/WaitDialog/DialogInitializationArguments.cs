using System;

namespace FocLauncherApp.WaitDialog
{
    [Serializable]
    public class DialogInitializationArguments
    {
        public string AppName { get; set; }

        public IntPtr AppMainWindowHandle { get; set; }

        public int AppProcessId { get; set; }
    }
}