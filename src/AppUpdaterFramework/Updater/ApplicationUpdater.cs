using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.AppUpaterFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

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

    public async Task<object> UpdateAsync(CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            await UpdateCoreAsync(token);
            return null;
        }
        catch (Exception e)
        {
            _logger?.LogError($"Update failed: {e.Message}");
            if (ShouldRethrowEngineException(e)) 
                ExceptionDispatchInfo.Capture(e).Throw();

            return null; // TODO
        }
    }

    private async Task UpdateCoreAsync(CancellationToken token)
    {
        try
        {
            if (!_updateCatalog.UpdateItems.Any())
                throw new InvalidOperationException("Nothing to update!");

            await Task.Run(() =>
            {
                var updateJob = new UpdateJob(_updateCatalog, this, _serviceProvider);
                updateJob.Schedule();
                _logger?.LogTrace("Starting update");
                updateJob.Run(token);
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