using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FocLauncher.Mods;
using FocLauncher.Properties;

namespace FocLauncher.Theming
{
    public class ThemeManager
    {
        private ITheme _theme;

        private readonly ContentControl _mainWindow;

        public static ThemeManager Instance
        {
            get => _instance ?? throw new InvalidOperationException("Theme Manager is not initialized");
        }

        public ObservableCollection<ITheme> Themes { get; }

        private readonly Dictionary<IMod, ITheme> _modThemeMapping = new Dictionary<IMod, ITheme>();
        private static ThemeManager _instance;

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
            var defaultTheme = Settings.Default.DefaultTheme;
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
            if (changeSettings && Settings.Default.SaveDefaultTheme)
                Settings.Default.DefaultTheme = theme.Name;
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
            return _modThemeMapping.TryGetValue(mod, out theme);
        }
    }
}
