using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FocLauncher.Mods;

namespace FocLauncher.Theming
{
    public class ThemeManager : IThemeManager
    {
        private ITheme _theme;

        private readonly ContentControl _mainWindow;

        public static ThemeManager Instance => _instance ?? throw new InvalidOperationException("Theme Manager is not initialized");

        public ObservableCollection<ITheme> Themes { get; }

        private readonly Dictionary<IMod, ITheme> _modThemeMapping = new Dictionary<IMod, ITheme>();
        private static ThemeManager? _instance;

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        public ITheme Theme
        {
            get => _theme;
            set
            {
                if (value == null)
                    throw new NoNullAllowedException();
                if (Equals(value, _theme))
                    return;
                RegisterTheme(value);
                var theme = _theme;
                _theme = value;
                ChangeTheme(theme, _theme, true);
                OnThemeChanged(value);
            }
        }

        private ThemeManager(ContentControl mainWindow)
        {
            _mainWindow = mainWindow;
            var defaultTheme = new DefaultTheme();
            Themes = new ObservableCollection<ITheme> { defaultTheme };
            ChangeTheme(null, defaultTheme, false);
            _theme = defaultTheme;
        }

        public void RegisterTheme(ITheme theme)
        {
            if (Themes.Contains(theme))
                return;
            Themes.Add(theme);
        }

        public uint GetThemedColorRgba(ComponentResourceKey componentResourceKey)
        {
            if (componentResourceKey is null)
                return 0;

            var rd = new ResourceDictionary
            {
                Source = Theme.GetResourceUri()
            };

            if (rd.Contains(componentResourceKey))
            {
                var themedObject = rd[componentResourceKey];
                if (themedObject is Color color)
                    return (uint) (color.A << 24 | color.B << 16 | color.G << 8) | color.R;
                if (themedObject is SolidColorBrush colorBrush)
                    return (uint) (colorBrush.Color.A << 24 | colorBrush.Color.B << 16 | colorBrush.Color.G << 8) |
                           colorBrush.Color.R;
            }
            return 0;
        }

        public static void Initialize(ContentControl mainWindow)
        {
            _instance = new ThemeManager(mainWindow);
        }

        public static ITheme GetThemeFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var assembly = Assembly.LoadFrom(filePath);
                var type = assembly.GetType($"{fileName}.Theme");
                var theme = (ITheme) Activator.CreateInstance(type);
                return theme;
            }
            catch
            {
                return null;
            }
        }

        public void ApplySavedDefaultTheme()
        {
            var defaultTheme = Properties.Settings.Default.DefaultTheme;
            var theme = Themes.FirstOrDefault(x => x.Name == defaultTheme);
            if (theme != null)
                Theme = theme;
        }

        private void ChangeTheme(ITheme oldTheme, ITheme theme, bool changeSettings)
        {
            var resources = Application.Current.Resources;
            if (oldTheme != null)
            {
                var oldResourceDict = Application.LoadComponent(oldTheme.GetResourceUri()) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Remove(oldResourceDict);
                _mainWindow?.Resources.MergedDictionaries.Remove(oldResourceDict);
            }

            if (theme == null)
                return;

            var newResourceDict = Application.LoadComponent(theme.GetResourceUri()) as ResourceDictionary;
            resources.MergedDictionaries.Add(newResourceDict);
            _mainWindow?.Resources.MergedDictionaries.Add(newResourceDict);
            if (changeSettings && Properties.Settings.Default.SaveDefaultTheme)
                Properties.Settings.Default.DefaultTheme = theme.Name;
        }

        public void AssociateThemeToMod(IMod mod, ITheme theme)
        {
            if (_modThemeMapping.ContainsKey(mod))
                _modThemeMapping[mod] = theme;
            else
                _modThemeMapping.Add(mod, theme);
        }

        public bool TryGetThemeByMod(IMod mod, out ITheme theme)
        {
            theme = default;
            return mod != null && _modThemeMapping.TryGetValue(mod, out theme);
        }

        protected virtual void OnThemeChanged(ITheme theme)
        {
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }
    }
}
