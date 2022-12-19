using System.Threading;
using System.Threading.Tasks;
using Sklavenwalker.ProductMetadata;

namespace Sklavenwalker.ProductUpdater.Services;

public interface IProductUpdateProviderService
{
    bool IsCheckingForUpdates { get; }

    Task<UpdateCheckResult> CheckForUpdates(IProductReference productReference, CancellationToken token = default);
}