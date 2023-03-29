using System;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.ExternalUpdater;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

public class ExternalUpdaterResultHandler
{
    private readonly IApplicationUpdaterRegistry _registry;

    public ExternalUpdaterResultHandler(IApplicationUpdaterRegistry registry)
    {
        Requires.NotNull(registry, nameof(registry));
        _registry = registry;
    }

    public void Handle(ExternalUpdaterResult result)
    {
        switch (result)
        {
            case ExternalUpdaterResult.UpdateFailedNoRestore:
                _registry.ScheduleReset();
                break;
            case ExternalUpdaterResult.UpdateFailedWithRestore:
            case ExternalUpdaterResult.UpdateSuccess:
                _registry.Clear();
                break;
            case ExternalUpdaterResult.UpdaterNotRun:
                break;
            case ExternalUpdaterResult.Restarted:
                // Safeguard, since Restarted makes no sense when an update should be performed.
                // Apparently something went wrong, so we reset the application
                if (_registry.RequiresUpdate)
                    _registry.ScheduleReset();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}