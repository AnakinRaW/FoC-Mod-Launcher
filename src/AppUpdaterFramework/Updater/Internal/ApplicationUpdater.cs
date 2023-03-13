using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class ApplicationUpdater : IApplicationUpdater, IProgressReporter
{
    private readonly IUpdateCatalog _updateCatalog;
    private readonly IServiceProvider _serviceProvider;
    public event EventHandler<ProgressEventArgs?>? Progress;

    private readonly ILogger? _logger;

    public ApplicationUpdater(IUpdateCatalog updateCatalog, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _updateCatalog = updateCatalog;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void Report(string package, double progress, ProgressType type, ProgressInfo detailedProgress)
    {
        Progress?.Invoke(this, new ProgressEventArgs(package, progress, type, detailedProgress));
    }

    public async Task<UpdateResult> UpdateAsync(CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var updateResult = await UpdateCoreAsync(token).ConfigureAwait(false);

            if (updateResult.RestartType == RestartType.ApplicationRestart)
                return updateResult;

            if (updateResult.Exception is not null)
                await RestoreBackups();

            try
            {
                await CleanUpdateData();
            }
            catch (Exception e)
            {
                _logger?.LogTrace(e, $"Failed to clean update data: {e.Message}");
            }
            
            return updateResult;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Update operation failed with error: {e.Message}");
            return CreateResult(e);
        }
    }

    private async Task<UpdateResult> UpdateCoreAsync(CancellationToken token)
    {
        try
        {
            if (!_updateCatalog.UpdateItems.Any())
                throw new InvalidOperationException("Nothing to update!");

            await Task.Run(() =>
            {
                using var updateJob = new UpdateJob(_updateCatalog, this, _serviceProvider);
                updateJob.Plan();
                // TODO: PreChecks
                try
                {
                    _logger?.LogTrace("Starting update");
                    updateJob.Run(token);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "Failed update: " + e.Message);
                    throw;
                }
                finally
                {
                    _logger?.LogTrace("Completed update");
                }
            }, CancellationToken.None).ConfigureAwait(false);
            
            return CreateResult();
        }
        catch (OperationCanceledException e)
        {
            _logger?.LogTrace("User canceled the update.");
            return CreateResult(e);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Update operation failed with error: {e.Message}");
            return CreateResult(e);
        }
    }

    private async Task RestoreBackups()
    {
        await Task.Run(() =>
        {
            try
            {
                var backupManager = _serviceProvider.GetRequiredService<IBackupManager>();
                backupManager.RestoreAll();
            }
            catch (Exception ex)
            {
                var e = new FailedRestoreException(ex);
                _logger?.LogTrace(e, $"Failed to restore from failed update : {e.Message}");
                throw e;
            }
        }).ConfigureAwait(false);
    }

    private async Task CleanUpdateData()
    {
        await Task.Run(() =>
        {
            new UpdateCleanJob(_serviceProvider).Run();
        }).ConfigureAwait(false);
    }


    private UpdateResult CreateResult(Exception? exception = null)
    {
        var restartType = _serviceProvider.GetRequiredService<IRestartManager>().RequiredRestartType;
        var requiresElevation = _serviceProvider.GetRequiredService<IElevationManager>().IsElevationRequested;
        var result = new UpdateResult
        {
            Exception = exception,
            IsCanceled = exception?.IsOperationCanceledException() ?? false,
            RestartType = restartType,
            RequiresElevation = requiresElevation,
            FailedRestore = exception?.IsExceptionType<FailedRestoreException>() ?? false
        };
        return result;
    }
}