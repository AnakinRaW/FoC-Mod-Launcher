using System;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.ExternalUpdater.CLI;
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
            case ExternalUpdaterResult.NoUpdate:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }
}