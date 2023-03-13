using System;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater.Backup;

internal class FailedRestoreException : UpdaterException
{
    public override string Message => $"Restore failed: {InnerException}";

    public FailedRestoreException(Exception innerException) : base("Update restore failed", innerException)
    {
        Requires.NotNull(innerException, nameof(innerException));
    }
}