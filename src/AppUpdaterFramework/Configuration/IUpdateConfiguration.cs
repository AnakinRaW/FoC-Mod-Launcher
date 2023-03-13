using System.IO;

namespace AnakinRaW.AppUpdaterFramework.Configuration;

public interface IUpdateConfiguration
{
    byte DownloadRetryCount { get; }

    string DownloadLocation { get; }

    string BackupLocation { get; }

    BackupPolicy BackupPolicy { get; }

    bool SupportsRestart { get; }

    bool ValidateInstallation { get; }
}

public sealed record UpdateConfiguration : IUpdateConfiguration
{
    internal static readonly IUpdateConfiguration Default = new UpdateConfiguration
    {
        DownloadRetryCount = 3,
        DownloadLocation = Path.GetTempPath(),
        BackupLocation = Path.GetTempPath(),
        BackupPolicy = BackupPolicy.NotRequired,
    };

    public byte DownloadRetryCount { get; init; }

    public required string DownloadLocation { get; init; }

    public BackupPolicy BackupPolicy { get; init; }

    public bool SupportsRestart { get; init; }

    public string BackupLocation { get; init; }

    public bool ValidateInstallation { get; init; }
}