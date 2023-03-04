using System;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal interface IInstaller
{
    event EventHandler<ProgressEventArgs> Progress;

    InstallResult Install(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token = default);

    InstallResult Remove(IInstallableComponent component, ProductVariables variables, CancellationToken token = default);
}