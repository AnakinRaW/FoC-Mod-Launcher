namespace FocLauncher.WaitDialog
{
    public class WaitDialogFactory
    {
        private static WaitDialogFactory _instance;

        public static WaitDialogFactory Instance => _instance ??= new WaitDialogFactory();

        public void CreateInstance(out IWaitDialog dialog)
        {
            dialog = new WaitDialogServiceWrapper();
        }
    }
}
