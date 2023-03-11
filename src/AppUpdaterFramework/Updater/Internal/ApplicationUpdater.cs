using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Restart;
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
            await UpdateCoreAsync(token).ConfigureAwait(false);

            _serviceProvider.GetRequiredService<IElevationManager>().SetElevationRequest();

            return CreateResult();
        }
        catch (Exception e) when (e.IsOperationCanceledException())
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

    private UpdateResult CreateResult(Exception? exception = null)
    {
        var restartType = _serviceProvider.GetRequiredService<IRestartManager>().RequiredRestartType;
        var requiresElevation = _serviceProvider.GetRequiredService<IElevationManager>().IsElevationRequested;
        var result = new UpdateResult
        {
            Exception = exception,
            IsCanceled = exception?.IsOperationCanceledException() ?? false,
            RestartType = restartType,
            RequiresElevation = requiresElevation
        };
        return result;
    }

    private async Task UpdateCoreAsync(CancellationToken token)
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
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, ex.Message);
            throw;
        }
    }
}