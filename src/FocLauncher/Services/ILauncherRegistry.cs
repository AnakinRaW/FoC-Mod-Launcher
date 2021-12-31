namespace FocLauncher.Services;

internal interface ILauncherRegistry
{
    /// <summary>
    /// Indicates the launcher shall get restored on next start.
    /// </summary>
    bool Restore { get; set; }

    void Reset();
}