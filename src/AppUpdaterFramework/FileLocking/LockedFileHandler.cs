using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;
using Vanara.PInvoke;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal class LockedFileHandler : InteractiveHandlerBase, ILockedFileHandler
{
    private readonly IUpdateConfiguration _updateConfiguration;
    private readonly ILockingProcessManagerFactory _lockingProcessManagerFactory;

    public LockedFileHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _updateConfiguration = serviceProvider.GetRequiredService<IUpdateConfigurationProvider>().GetConfiguration();
        _lockingProcessManagerFactory = serviceProvider.GetRequiredService<ILockingProcessManagerFactory>();
    }

    public ILockedFileHandler.Result Handle(IInstallableComponent component, IFileInfo file)
    {
        Requires.NotNull(component, nameof(component));
        Requires.NotNull(component, nameof(component));
        Assumes.True(file.Exists, $"Expected '{file}' to exist.");

        using var lockingProcessManager = _lockingProcessManagerFactory.Create();
        lockingProcessManager.Register(new[] { file.FullName });

        var lockingProcesses = lockingProcessManager.GetProcesses().ToList();

        if (!lockingProcesses.Any())
        {
            var e = new InvalidOperationException($"The file '{file}' is not locked by any process.");
            Logger?.LogTrace(e, e.Message);
            throw e;
        }

        var isLockedByApplication = lockingProcesses.ContainsCurrentProcess();

        //The file is locked by a system file
        if (lockingProcesses.Any(x => x.ApplicationType == RstrtMgr.RM_APP_TYPE.RmCritical && !x.IsCurrentProcess()))
        {
            PromptError("Files are locked by a system process that cannot be terminated. Please restart the system");
            return ILockedFileHandler.Result.Locked;
        }


        Logger?.LogTrace($"Handling locked file '{file}'");

        var processesWithoutSelf = lockingProcesses.Where(x => !x.IsCurrentProcess());
        using var lockingProcessManagerWithoutSelf = _lockingProcessManagerFactory.Create();
        lockingProcessManagerWithoutSelf.Register(null, processesWithoutSelf);

        if (!PromptProcessKill(file, lockingProcesses))
        {
            Logger?.LogTrace($"Interaction result: Locked file '{file}' shall not be unlocked.");
            return ILockedFileHandler.Result.Locked;
        }

        lockingProcessManagerWithoutSelf.TerminateRegisteredProcesses();

        // File is still locked
        if (lockingProcessManagerWithoutSelf.GetProcesses().Any())
            return ILockedFileHandler.Result.Locked;


        if (isLockedByApplication)
        {
            if (_updateConfiguration.SupportsRestart)
                return ILockedFileHandler.Result.Locked;

            Logger?.LogTrace($"File '{file}' is locked by current application and get schedule for restart.");
            return ILockedFileHandler.Result.RequiresRestart;
        }

        return ILockedFileHandler.Result.Unlocked;
    }

    private bool PromptProcessKill(IFileInfo file, List<ILockingProcessInfo> lockingProcesses)
    {
        //var supportedStates = new SupportedInteractions(
        //    new SupportedInteractionState(InteractionStatus.Retry, ""));

        //if (!supportedStates.Contains(interactionStatus))
        //    throw new InvalidOperationException("Retrieved interaction status was not expected.");

        return false;
    }
}