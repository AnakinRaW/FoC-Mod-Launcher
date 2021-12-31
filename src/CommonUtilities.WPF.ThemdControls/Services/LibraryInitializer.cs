using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Theming;

namespace Sklavenwalker.CommonUtilities.Wpf.Services
{
    public static class LibraryInitializer
    {
        public static IServiceCollection InitializeLibrary(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IThemeResourceDictionaryCache>(sp => new ThemeResourceDictionaryCache(sp));
            serviceCollection.AddTransient<IThemeResourceDictionaryBuilder>(_ => new ThemeResourceDictionaryBuilder());
            return serviceCollection;
        }
    }
}
