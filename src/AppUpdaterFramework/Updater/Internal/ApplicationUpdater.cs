using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
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
            try
            {
                token.ThrowIfCancellationRequested();
                await UpdateCoreAsync(token).ConfigureAwait(false);
                return CreateResult();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Update failed: {e.Message}");
                if (ShouldRethrowEngineException(e))
                    ExceptionDispatchInfo.Capture(e).Throw();
                return CreateResult(e);
            }
        }
        catch (Exception e) when (e.IsOperationCanceledException())
        {
            _logger?.LogTrace("User canceled the update.");
            return new UpdateResult
            {
                IsCanceled = true,
                Exception = e
            };
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Update operation failed with error: {e.Message}");
            return new UpdateResult
            {
                Exception = e
            };
        }
    }

    private static UpdateResult CreateResult(Exception? exception = null)
    {
        var result = new UpdateResult
        {
            Exception = exception,
            IsCanceled = exception?.IsOperationCanceledException() ?? false
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

    private static bool ShouldRethrowEngineException(Exception ex)
    {
        return ex is not ComponentFailedException && !ex.IsOperationCanceledException();
    }
}