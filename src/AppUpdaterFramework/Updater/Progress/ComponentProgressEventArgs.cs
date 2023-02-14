using System;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class ComponentProgressEventArgs : EventArgs
{
    public string ComponentInfo { get; }

    public double Progress { get; }
}