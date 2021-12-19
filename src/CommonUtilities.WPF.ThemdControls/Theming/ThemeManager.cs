using System;
using System.Windows;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public class ThemeManager : IThemeManager
{
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private Application _application;
    private Window _mainWindow;
    private ITheme _theme;

    private bool _initialized;

    public ITheme Theme
    {
        get => _theme;
        set
        {
            ThrowIfNotInitialized();
            if (value == null)
                throw new InvalidOperationException("Theme must not be null");
            if (Equals(value, _theme))
                return;
            var oldTheme = _theme;
            _theme = value;
            ChangeTheme(oldTheme, _theme);
            OnRaiseThemeChanged(new ThemeChangedEventArgs(value, oldTheme));
        }
    }

    public void Initialize(Application application, Window mainWindow, ITheme? defaultTheme = null)
    {
        if (_initialized)
            throw new InvalidOperationException("Theme manager already initialized");
        Requires.NotNull(application, nameof(application));
        Requires.NotNull(mainWindow, nameof(mainWindow));
        lock (this)
        {
            _application = application;
            _mainWindow = mainWindow;
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
        var windowResources = _mainWindow.Resources;

        if (oldTheme is not null)
        {
            var oldResourceDict = Application.LoadComponent(oldTheme.ResourceUri) as ResourceDictionary;
            resources.MergedDictionaries.Remove(oldResourceDict);
            windowResources.MergedDictionaries.Remove(oldResourceDict);
        }

        var newResourceDict = Application.LoadComponent(theme.ResourceUri) as ResourceDictionary;
        resources.MergedDictionaries.Add(newResourceDict);
        windowResources.MergedDictionaries.Add(newResourceDict);
    }

    protected void ThrowIfNotInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("ThemeManager must be initialized first.");
    }
}