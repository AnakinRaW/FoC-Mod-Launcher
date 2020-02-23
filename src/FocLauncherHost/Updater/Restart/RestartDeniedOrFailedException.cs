namespace FocLauncherHost.Updater.Restart
{
    public class RestartDeniedOrFailedException : UpdaterException
    {
        public RestartDeniedOrFailedException(string message) : base(message)
        {
        }
    }
}
