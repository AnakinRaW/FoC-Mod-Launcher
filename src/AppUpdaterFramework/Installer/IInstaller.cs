using System;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Installer;

internal interface IInstaller
{
    event EventHandler<ProgressEventArgs> Progress;

    InstallResult Install(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token = default);

    InstallResult Remove(IInstallableComponent component, ProductVariables variables, CancellationToken token = default);
}