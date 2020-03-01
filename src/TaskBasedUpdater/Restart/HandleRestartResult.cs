using System;

namespace TaskBasedUpdater.Restart
{
    public struct HandleRestartResult
    {
        public string Message { get; }
        public HandleRestartStatus Status { get; }

        public HandleRestartResult(HandleRestartStatus status, string message)
        {
            Message = message;
            Status = status;
        }

        public HandleRestartResult(HandleRestartStatus status) : this(status, String.Empty)
        {
        }
    }
}