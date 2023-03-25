using System.IO.Abstractions;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;

public interface IApplicationUpdaterRegistry
{
    /// <summary>
    /// Indicates the launcher shall get restored on next start.
    /// </summary>
    bool Restore { get; }

    bool RequiresUpdate { get; }

    string? UpdateCommandArgs { get; }

    string? UpdaterPath { get; }

    void ScheduleRestore();

    void Reset();

    void ScheduleUpdate(IFileInfo updater, ExternalUpdaterArguments arguments);
}