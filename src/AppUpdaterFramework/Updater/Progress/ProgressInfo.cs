namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

public struct ProgressInfo
{
    public ProgressInfo(int currentComponent, int totalComponents, long downloadedSize, long totalSize, long downloadSpeed)
    {
        CurrentComponent = currentComponent;
        TotalComponents = totalComponents;
        DownloadedSize = downloadedSize;
        TotalSize = totalSize;
        DownloadSpeed = downloadSpeed;
    }

    public int CurrentComponent { get; internal set; }

    public int TotalComponents { get; internal set; }

    public long DownloadedSize { get; internal set; }

    public long TotalSize { get; internal set; }

    public long DownloadSpeed { get; internal set; }

    public override string ToString() =>
        $"Package={CurrentComponent},TotalComponents={TotalComponents},DownloadedSize={DownloadedSize},Total={TotalSize},DownloadSpeed={DownloadSpeed}";
}