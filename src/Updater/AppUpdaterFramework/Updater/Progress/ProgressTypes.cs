using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal static class ProgressTypes
{
    public static readonly ProgressType Install = new()
    {
        Id = "install",
        DisplayName = "Install"
    };

    public static readonly ProgressType Download = new()
    {
        Id = "download",
        DisplayName = "Download"
    };

    public static readonly ProgressType Verify = new()
    {
        Id = "verify",
        DisplayName = "Verify"
    };
}