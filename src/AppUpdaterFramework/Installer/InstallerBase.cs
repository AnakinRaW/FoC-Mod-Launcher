using System;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal abstract class InstallerBase : IInstaller
{
    public event EventHandler<ProgressEventArgs>? Progress;

    protected readonly ILogger? Logger;

    protected InstallerBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public InstallResult Install(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token = default)
    {
        return ExecuteInstallerAction(component, source, InstallAction.Install, variables, token);
    }

    public InstallResult Remove(IInstallableComponent component, ProductVariables variables, CancellationToken token = default)
    {
        return ExecuteInstallerAction(component, null, InstallAction.Remove, variables, token);
    }

    protected abstract InstallResult RemoveCore(IInstallableComponent component, ProductVariables variables, CancellationToken token);

    protected abstract InstallResult InstallCore(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token);


    private InstallResult ExecuteInstallerAction(IInstallableComponent component, string? source, InstallAction action, ProductVariables variables, CancellationToken token)
    {
        try
        {
            OnProgress(component, 0.0);
            try
            {
                Logger?.LogInformation($"Started: {action}ing {component.GetDisplayName()}");
                switch (action)
                {
                    case InstallAction.Install:
                        return InstallCore(component, source!, variables, token);
                    case InstallAction.Remove:
                        return RemoveCore(component, variables, token);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
            }
            finally
            {
                Logger?.LogInformation($"Completed: {action}ing {component.GetDisplayName()}");
            }
        }
        catch (OperationCanceledException)
        {
            Logger?.LogInformation($"User canceled during component {action}.");
            return InstallResult.Cancel;
        }
        catch (UnauthorizedAccessException e)
        {
            LogFailure(component, action, e.ToString());
            return InstallResult.FailureElevationRequired;
        }
        catch (Exception e)
        {
            LogFailure(component, action, e.ToString());
            return InstallResult.Failure;
        }
        finally
        {
            OnProgress(component, 1.0);
        }
    }

    protected InstallResult ExecuteWithInteractiveRetry(
        IInstallableComponent component,
        Func<InstallOperationResult> action,
        Func<InstallOperationResult, InstallerInteractionResult> interaction,
        CancellationToken token)
    {
        InstallResult result;
        var retry = false;
        do
        {
            token.ThrowIfCancellationRequested();

            if (retry)
                Logger?.LogTrace("Retrying action for package '" + component.GetUniqueId() + "' per client's request.");

            OnProgress(component, 0.0);
            var operationResult = action();

            switch (operationResult)
            {
                case InstallOperationResult.Success:
                    return InstallResult.Success;
                case InstallOperationResult.Canceled:
                    return InstallResult.Cancel;
                case InstallOperationResult.Failed:
                    return InstallResult.Failure;
                case InstallOperationResult.NoPermission:
                    return InstallResult.FailureElevationRequired;
                case InstallOperationResult.LockedFile:
                default:
                    var interactionResult = interaction(operationResult);
                    result = interactionResult.InstallResult;
                    retry = interactionResult.Retry;
                    break;
            }
        } while (retry);

        return result;
    }

    private void OnProgress(IInstallableComponent component, double progress)
    {
        Progress?.Invoke(this, new ProgressEventArgs(component.GetDisplayName(), progress, ProgressType.Install));
    }

    private void LogFailure(IProductComponent? component, InstallAction executeAction, string details)
    {
        Logger?.LogError(component != null
            ? $"Component '{component.GetDisplayName()}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
            : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
    }

    private protected enum InstallAction
    {
        Install,
        Remove
    }
}