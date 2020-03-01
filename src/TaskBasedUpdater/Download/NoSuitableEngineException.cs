using System;

namespace TaskBasedUpdater.Download
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