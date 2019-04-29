using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FocLauncher.Core.Mods;
using FocLauncher.Core.Properties;
using FocLauncher.Theming;

namespace FocLauncher.Core.Theming
{
    public class ThemeManager
    {
        private static ThemeManager _instance;
        private ITheme _theme;

        public static ThemeManager Instance => _instance ?? (_instance = new ThemeManager());

        public ObservableCollection<ITheme> Themes { get; }

        private readonly Dictionary<IMod, ITheme> _modThemeMapping = new Dictionary<IMod, ITheme>();

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

        private ThemeManager()
        {
            Themes = new ObservableCollection<ITheme> {new DefaultTheme()};
            ChangeTheme(null, new DefaultTheme(), false);
        }

        public void RegisterTheme(ITheme theme)
        {
            if (Themes.Contains(theme))
                return;
            Themes.Add(theme);
        }

        public static void Initialize()
        {
            var _ = Instance;
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
            resources.Clear();
            resources.MergedDictionaries.Clear();
            if (oldTheme != null)
            {
                var resourceDictionary =
                    resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
                if (resourceDictionary != null)
                    resources.MergedDictionaries.Remove(resourceDictionary);
            }

            if (theme == null)
                return;
            resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = theme.GetResourceUri()
            });
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
