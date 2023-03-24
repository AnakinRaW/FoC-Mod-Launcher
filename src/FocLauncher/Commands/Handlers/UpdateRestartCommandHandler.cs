using System;
using System.Diagnostics;
using System.IO;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Commands.Handlers;

internal class UpdateRestartCommandHandler : CommandHandlerBase<ExternalUpdateOptions>, IUpdateRestartCommandHandler
{
    private readonly IExternalUpdaterService _updaterService;
    private readonly IProductService _productService;
    private readonly IApplicationShutdownService _shutdownService;

    public UpdateRestartCommandHandler(IServiceProvider serviceProvider)
    {
        _updaterService = serviceProvider.GetRequiredService<IExternalUpdaterService>();
        _productService = serviceProvider.GetRequiredService<IProductService>();
        _shutdownService = serviceProvider.GetRequiredService<IApplicationShutdownService>();
    }

    public override void Handle(ExternalUpdateOptions parameter)
    {
        var updater = _updaterService.GetExternalUpdater();
        if (!updater.Exists)
            throw new FileNotFoundException("Could not find external updater application", updater.FullName);


        var externalUpdateStartInfo = new ProcessStartInfo(updater.FullName)
        {
#if !DEBUG
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
        };


        // TODO: Set registry but remove wait pid
    }
}