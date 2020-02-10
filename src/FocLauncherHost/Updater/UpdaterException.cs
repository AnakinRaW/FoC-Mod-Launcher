using System;
using System.Runtime.Serialization;

namespace FocLauncherHost.Updater
{
    [Serializable]
    internal class UpdaterException : Exception
    {
        public UpdaterException()
        {
        }

        public UpdaterException(string message) : base(message)
        {
        }

        public UpdaterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UpdaterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
