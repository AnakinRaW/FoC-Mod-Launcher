using System;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Commands.Handlers;

internal class UpdateRestartCommandHandler : CommandHandlerBase<ExternalUpdaterArguments>, IUpdateRestartCommandHandler
{
    private readonly IExternalUpdaterService _updaterService;
    private readonly IApplicationShutdownService _shutdownService;

    public UpdateRestartCommandHandler(IServiceProvider serviceProvider)
    {
        _updaterService = serviceProvider.GetRequiredService<IExternalUpdaterService>();
        _shutdownService = serviceProvider.GetRequiredService<IApplicationShutdownService>();
    }

    public override void Handle(ExternalUpdaterArguments arguments)
    {
        _updaterService.Launch(arguments);
        _shutdownService.Shutdown(0);
    }
}