using System;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Commands.Factories;

internal class UpdateCommandsFactory : IUpdateCommandsFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UpdateCommandsFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    public ICommandDefinition CreateRestart()
    {
        var arguments = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateUpdateArguments();
        return new UpdateRestartCommand(arguments, _serviceProvider);
    }

    public ICommandDefinition CreateElevate()
    {
        return new ElevateApplicationCommand(_serviceProvider);
    }
}