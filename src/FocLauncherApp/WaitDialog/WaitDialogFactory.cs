namespace FocLauncherApp.WaitDialog
{
    internal static class WaitDialogFactory
    {
        public static IWaitDialog CreateInstance()
        {
            return new WaitDialogServiceWrapper();
        }
    }
}
