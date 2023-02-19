using System.IO;

namespace AnakinRaW.AppUpaterFramework.Updater.Configuration;

public interface IUpdateConfiguration
{
    byte DownloadRetryCount { get; }

    string TempDownloadLocation { get; }
}

public sealed record UpdateConfiguration : IUpdateConfiguration
{
    internal static readonly IUpdateConfiguration Default = new UpdateConfiguration
    {
        DownloadRetryCount = 3,
        TempDownloadLocation = Path.GetTempPath()
    };

    public byte DownloadRetryCount { get; init; }

    public required string TempDownloadLocation { get; init; }
}