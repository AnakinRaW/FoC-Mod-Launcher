namespace FocLauncher.Shared;

internal record LauncherUpdateItem
{
    public string? File { get; set; }

    public string? Destination { get; set; }

    public string? BackupDestination { get; set; }

    public string? Backup { get; set; }

}