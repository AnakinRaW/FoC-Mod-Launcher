using System;
using System.IO.Abstractions;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal interface IInstaller
{
    event EventHandler<ComponentProgressEventArgs?> Progress;

    InstallResult Install(IInstallableComponent component, IFileInfo? source, ProductVariables variables, CancellationToken token = default);

    InstallResult Remove(IInstallableComponent component, ProductVariables variables, CancellationToken token = default);
}