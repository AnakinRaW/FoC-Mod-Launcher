using System;
using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal interface IInstaller
{
    event EventHandler<ProgressEventArgs> Progress;
}