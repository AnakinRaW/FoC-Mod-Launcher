using System;
using System.Net;
using System.Runtime.Serialization;

namespace TaskBasedUpdater.Download
{
    [Serializable]
    public class WrappedWebException : WebException
    {
        public WrappedWebException(int errorCode, string message)
            : base(message)
        {
            HResult = errorCode;
        }

        protected WrappedWebException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}