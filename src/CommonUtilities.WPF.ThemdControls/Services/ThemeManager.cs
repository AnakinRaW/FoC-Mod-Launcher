using System;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Theming;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Services;

public class ThemeManager : IThemeManager
{
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private Application _application;
    private ITheme _theme;

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

    public ThemeManager()
    {
        _cache = new ThemeResourceDictionaryCache();
    }

    public void Initialize(Application application,ITheme? defaultTheme = null)
    {
        if (_initialized)
            throw new InvalidOperationException("Theme manager already initialized");
        Requires.NotNull(application, nameof(application));
        lock (this)
        {
            _application = application;
            _theme = defaultTheme ?? new FallbackTheme();
            ChangeTheme(null, _theme);
            _initialized = true;
        }
    }
    
    protected virtual void OnRaiseThemeChanged(ThemeChangedEventArgs e)
    {
        var handler = ThemeChanged;
        handler?.Invoke(this, e);
    }

    private void ChangeTheme(ITheme? oldTheme, ITheme theme)
    {
        var resources = _application.Resources;

        if (oldTheme is not null)
        {
            var oldResourceDict = _cache.Get(oldTheme);
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