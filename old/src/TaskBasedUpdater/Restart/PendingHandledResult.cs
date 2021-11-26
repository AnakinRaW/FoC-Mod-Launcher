using System;

namespace TaskBasedUpdater.Restart
{
    public struct PendingHandledResult
    {
        public string Message { get; }
        public HandlePendingComponentsStatus Status { get; }

        public PendingHandledResult(HandlePendingComponentsStatus status, string message)
        {
            Message = message;
            Status = status;
        }

        public PendingHandledResult(HandlePendingComponentsStatus status) : this(status, String.Empty)
        {
        }
    }
}