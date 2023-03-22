using System;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal class ComponentProgressEventArgs : EventArgs
{
    public string ComponentInfo { get; }

    public double Progress { get; }
}