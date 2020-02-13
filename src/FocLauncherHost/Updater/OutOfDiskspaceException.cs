namespace FocLauncherHost.Updater
{
    public class OutOfDiskspaceException : UpdaterException
    {
        public OutOfDiskspaceException() : base(nameof(OutOfDiskspaceException))
        {
        }
    }
}