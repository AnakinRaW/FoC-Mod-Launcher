using System.Threading;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Configuration;

public abstract class UpdateConfigurationProviderBase : IUpdateConfigurationProvider
{
    private IUpdateConfiguration _lazyConfiguration = null!;

    public IUpdateConfiguration GetConfiguration()
    {
        var configuration =  LazyInitializer.EnsureInitialized(ref _lazyConfiguration, CreateConfiguration);
        Assumes.NotNull(configuration);
        return configuration;
    }

    protected abstract IUpdateConfiguration CreateConfiguration();
}