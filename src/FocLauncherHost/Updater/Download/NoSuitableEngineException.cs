using System;

namespace FocLauncherHost.Updater.Download
{
    [Serializable]
    public class NoSuitableEngineException : InvalidOperationException
    {
        public NoSuitableEngineException(string message)
            : base(message)
        {
        }
    }
}