using System;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

public class ProgressEventArgs : EventArgs
{
    public string Component { get; }

    public double Progress { get; }

    public ProgressType Type { get; }

    public ProgressInfo DetailedProgress { get; }

    public ProgressEventArgs(string component, double progress, ProgressType type)
        : this(component, progress, type, new ProgressInfo())
    {
    }

    public ProgressEventArgs(string component, double progress, ProgressType type, ProgressInfo detailedProgress)
    {
        Requires.NotNullOrEmpty(component, nameof(component));
        Component = component;
        Progress = progress;
        Type = type;
        DetailedProgress = detailedProgress;
    }
}