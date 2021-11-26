using System;

namespace FocLauncher.Shared
{
    [Serializable]
    internal enum ExternalUpdaterResult
    {
        UpdateFailedNoRestore = -2,
        UpdateFailedWithRestore = -1,
        NoUpdate = 0,
        UpdateSuccess = 1,
        DemandsRestore = 2,
    }
}