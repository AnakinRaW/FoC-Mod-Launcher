using System;
using System.IO;

namespace TaskBasedUpdater
{
    public class RestoreFailedException : IOException
    {
        public RestoreFailedException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}