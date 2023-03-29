using System.IO.Abstractions;
using AnakinRaW.ExternalUpdater.Options;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;

public interface IApplicationUpdaterRegistry
{
    bool Reset { get; }

    bool RequiresUpdate { get; }

    string? UpdateCommandArgs { get; }

    string? UpdaterPath { get; }

    void ScheduleReset();

    void Clear();

    void ScheduleUpdate(IFileInfo updater, ExternalUpdaterOptions options);
}