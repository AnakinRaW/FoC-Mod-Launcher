using System;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.ExternalUpdater.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.Commands.Handlers;

internal class UpdateRestartCommandHandler : CommandHandlerBase<ExternalUpdaterOptions>, IUpdateRestartCommandHandler
{
    private readonly IExternalUpdaterService _updaterService;
    private readonly IApplicationShutdownService _shutdownService;

    public UpdateRestartCommandHandler(IServiceProvider serviceProvider)
    {
        _updaterService = serviceProvider.GetRequiredService<IExternalUpdaterService>();
        _shutdownService = serviceProvider.GetRequiredService<IApplicationShutdownService>();
    }

    public override void Handle(ExternalUpdaterOptions options)
    {
        _updaterService.Launch(options);
        _shutdownService.Shutdown(0);
    }
}