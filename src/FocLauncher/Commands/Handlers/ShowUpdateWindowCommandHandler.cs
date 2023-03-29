using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Utilities;
using Microsoft.Extensions.DependencyInjection;
using AnakinRaW.AppUpdaterFramework.ViewModels;
using AnakinRaW.CommonUtilities.Windows;
using AnakinRaW.ExternalUpdater;

namespace FocLauncher.Commands.Handlers;

internal class ShowUpdateWindowCommandHandler : AsyncCommandHandlerBase, IShowUpdateWindowCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionManager _connectionManager;

    public ShowUpdateWindowCommandHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
    }

    public override async Task HandleAsync()
    {
        await ExtractAssemblies();

        // Singletone instance of this view model drastically increases closing/cancellation complexity.
        // Creating a new model for each request should be good enough. 
        var viewModel = new UpdateWindowViewModel(_serviceProvider);
        await _serviceProvider.GetRequiredService<IModalWindowService>().ShowModal(viewModel);
    }

    protected override bool CanHandle()
    {
        return _connectionManager.HasInternetConnection();
    }

    private async Task ExtractAssemblies()
    {
        var env = _serviceProvider.GetRequiredService<ILauncherEnvironment>();
        await _serviceProvider.GetRequiredService<ICosturaResourceExtractor>()
            .ExtractAsync(ExternalUpdaterConstants.AppUpdaterModuleName, env.ApplicationLocalPath, ShouldOverwriteUpdater);
    }

    private static bool ShouldOverwriteUpdater(string filePath, Stream assemblyStream)
    {
        var resourceInfo = ExternalUpdaterInformation.FromAssemblyStream(assemblyStream);
        if (!Version.TryParse(FileVersionInfo.GetVersionInfo(filePath).FileVersion, out var installedVersion))
            return true;
        return resourceInfo.FileVersion > installedVersion;
    }
}