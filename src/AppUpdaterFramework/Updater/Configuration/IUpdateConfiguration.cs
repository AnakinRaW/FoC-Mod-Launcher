using System.IO;

namespace AnakinRaW.AppUpaterFramework.Updater.Configuration;

public interface IUpdateConfiguration
{
    byte DownloadRetryCount { get; }

    string TempDownloadLocation { get; }

    string BackupLocation { get; }

    BackupPolicy BackupPolicy { get; }
}

public sealed record UpdateConfiguration : IUpdateConfiguration
{
    internal static readonly IUpdateConfiguration Default = new UpdateConfiguration
    {
        DownloadRetryCount = 3,
        TempDownloadLocation = Path.GetTempPath(),
        BackupLocation = Path.GetTempPath(),
        BackupPolicy = BackupPolicy.NotRequired
    };

    public byte DownloadRetryCount { get; init; }

    public required string TempDownloadLocation { get; init; }

    public BackupPolicy BackupPolicy { get; init; }

    public string BackupLocation { get; init; }
}