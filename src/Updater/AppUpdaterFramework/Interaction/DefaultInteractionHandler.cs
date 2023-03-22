using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Interaction;


internal class DefaultInteractionHandler : IInteractionHandler
{
    private readonly ILogger? _logger;

    public DefaultInteractionHandler(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public LockedFileHandlerInteractionResult HandleLockedFile(IFileInfo file, IEnumerable<ILockingProcess> lockingProcesses)
    {
        _logger?.LogTrace($"Interaction Result: {LockedFileHandlerInteractionResult.Cancel}");
        return LockedFileHandlerInteractionResult.Cancel;
    }

    public void HandleError(string message)
    {
        _logger?.LogTrace($"Interaction Error: {message}");
    }
}