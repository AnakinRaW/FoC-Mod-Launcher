using System;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Themes;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

public class ThemeManager : IThemeManager
{
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private System.Windows.Application _application = null!;
    private ITheme _theme = null!;

    private readonly IThemeResourceDictionaryCache _cache;

    private bool _initialized;

    public ITheme Theme
    {
        get => _theme;
        set
        {
            if (value == null)
                throw new InvalidOperationException("Theme must not be null");
            ThrowIfNotInitialized();
            if (Equals(value, _theme))
                return;
            var oldTheme = _theme;
            _theme = value;
            ChangeTheme(oldTheme, _theme);
            OnRaiseThemeChanged(new ThemeChangedEventArgs(value, oldTheme));
        }
    }

    public ThemeManager(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _cache = serviceProvider.GetService<IThemeResourceDictionaryCache>() ?? new ThemeResourceDictionaryCache(serviceProvider);
    }

    public void Initialize(System.Windows.Application application, ITheme? defaultTheme = null)
    {
        if (_initialized)
            throw new InvalidOperationException("Theme manager already initialized");
        Requires.NotNull(application, nameof(application));
        lock (this)
        {
            _application = application;
            _theme = defaultTheme ?? new DefaultTheme();
            ChangeTheme(null, _theme);
            _initialized = true;
        }
    }
    
    protected virtual void OnRaiseThemeChanged(ThemeChangedEventArgs e)
    {
        ThemeChanged?.Invoke(this, e);
        ImageThemingUtilities.ClearWeakImageCache();
        ScrollBarThemingUtilities.OnThemeChanged();
    }

    private void ChangeTheme(ITheme? oldTheme, ITheme theme)
    {
        var resources = _application.Resources;

        if (oldTheme is not null)
        {
            var oldResourceDict = _cache.Get(oldTheme);
            if (oldResourceDict != null) 
                resources.MergedDictionaries.Remove(oldResourceDict);
        }

        var themeResources = _cache.GetOrCreate(theme);
        resources.MergedDictionaries.Add(themeResources);
    }

    protected void ThrowIfNotInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("ThemeManager must be initialized first.");
    }
}