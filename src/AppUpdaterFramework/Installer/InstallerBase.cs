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

    private readonly ILogger? _logger;

    protected InstallerBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
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
                _logger?.LogInformation($"Started: {action}ing {component.GetDisplayName()}");
                switch (action)
                {
                    case InstallAction.Install:
                        return InstallCore(component, source, variables, token);
                    case InstallAction.Remove:
                        return RemoveCore(component, variables, token);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
            }
            finally
            {
                _logger?.LogInformation($"Completed: {action}ing {component.GetDisplayName()}");
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation($"User canceled during component {action}.");
            return InstallResult.Cancel;
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
                _logger?.LogTrace("Retrying action for package '" + component.GetUniqueId() + "' per client's request.");

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
                default:
                    var interactionResult = interaction(operationResult);
                    result = interactionResult.InstallResult;
                    retry = interactionResult.Retry;
                    break;
            }
        } while (retry);

        if (result != InstallResult.SuccessRestartRequired)
        {
            // TODO: Add action to pending
        }

        return result;
    }

    private void OnProgress(IInstallableComponent component, double progress)
    {
        Progress?.Invoke(this, new ProgressEventArgs(component.GetDisplayName(), progress, ProgressType.Install));
    }

    private void LogFailure(IProductComponent? component, InstallAction executeAction, string details)
    {
        _logger?.LogError(component != null
            ? $"Component '{component.GetDisplayName()}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
            : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
    }

    private enum InstallAction
    {
        Install,
        Remove
    }
}