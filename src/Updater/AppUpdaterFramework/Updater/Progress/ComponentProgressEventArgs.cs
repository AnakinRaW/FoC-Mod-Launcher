using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

public class ComponentProgressEventArgs : ProgressEventArgs<ComponentProgressInfo>
{
    public ComponentProgressEventArgs(string progressText, double progress, ProgressType type) : base(progressText, progress, type)
    {
    }

    public ComponentProgressEventArgs(string progressText, double progress, ProgressType type, ComponentProgressInfo detailedProgress) : base(progressText, progress, type, detailedProgress)
    {
    }
}