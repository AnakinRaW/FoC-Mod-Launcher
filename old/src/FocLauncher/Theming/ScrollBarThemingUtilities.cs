﻿    using System;
    using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
    using FocLauncher.Utilities;

    namespace FocLauncher.Theming
{
    internal class ScrollBarThemingUtilities
    {
        private static readonly WeakCollection<FrameworkElement> WeakUnthemedScrollingElements = new();

        public static readonly DependencyProperty ThemeScrollBarsProperty = 
            DependencyProperty.RegisterAttached("ThemeScrollBars", typeof(bool?), typeof(ScrollBarThemingUtilities),
            new FrameworkPropertyMetadata(null, OnThemeScrollBarsChanged));

        public static void SetThemeScrollBars(Control element, bool? value)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(ThemeScrollBarsProperty, value);
        }

        public static bool? GetThemeScrollBars(DependencyObject element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            return (bool?)element.GetValue(ThemeScrollBarsProperty);
        }

        static ScrollBarThemingUtilities()
        {
            if (!(LauncherServiceProvider.Instance.GetService(typeof(IThemeManager)) is IThemeManager themeManager))
                return;
            themeManager.ThemeChanged += OnThemeChanged;
            RepublishUnthemedScrollBarStyles();
        }

        private static void UpdateUnthemedScrollBarStyles()
        {
            RepublishUnthemedScrollBarStyles();
            ReapplyUnthemedScrollBarStyles();
        }

        private static void RepublishApplicationResource(object existingKey, object newKey)
        {
            if (Application.Current == null)
                return;
            var resource = Application.Current.TryFindResource(existingKey);
            if (resource == null)
                return;
            Application.Current.Resources[newKey] = resource;
        }

        private static void RepublishUnthemedScrollBarStyles()
        {
            RepublishApplicationResource(typeof(ScrollBar), ScrollBarResourceKeys.UnthemedScrollBarStyleKey);
            RepublishApplicationResource(typeof(ScrollViewer), ScrollBarResourceKeys.UnthemedScrollViewerStyleKey);
            RepublishApplicationResource(GridView.GridViewScrollViewerStyleKey, ScrollBarResourceKeys.UnthemedGridViewScrollViewerStyleKey);
        }

        private static void ReapplyUnthemedScrollBarStyles()
        {
            foreach (var element in WeakUnthemedScrollingElements.ToList())
                SetScrollBarStyles(element, false);
        }

        private static void SetScrollBarStyles(FrameworkElement element, bool themeScrollBars)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            element.Resources[typeof(ScrollBar)] = element.TryFindResource(ScrollBarResourceKeys.GetScrollBarStyleKey(themeScrollBars));
            element.Resources[typeof(ScrollViewer)] = element.TryFindResource(ScrollBarResourceKeys.GetScrollViewerStyleKey(themeScrollBars));
            element.Resources[GridView.GridViewScrollViewerStyleKey] = element.TryFindResource(ScrollBarResourceKeys.GetGridViewScrollViewerStyleKey(themeScrollBars));
        }

        private static void OnPresentationSourceChanged(object sender, SourceChangedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
                UpdateScrollBarStyles(element);
            PresentationSource.RemoveSourceChangedHandler(element, OnPresentationSourceChanged);

        }

        private static void UpdateScrollBarStyles(FrameworkElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            var themeScrollBars = GetThemeScrollBars(element);
            if (themeScrollBars.HasValue)
            {
                SetScrollBarStyles(element, themeScrollBars.Value);
                if (themeScrollBars.Value)
                    WeakUnthemedScrollingElements.Remove(element);
                else
                    WeakUnthemedScrollingElements.Add(element);
            }
            else
            {
                RemoveScrollBarStyles(element);
                WeakUnthemedScrollingElements.Remove(element);
            }
        }

        private static void RemoveScrollBarStyles(FrameworkElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            element.Resources.Remove(typeof(ScrollBar));
            element.Resources.Remove(typeof(ScrollViewer));
            element.Resources.Remove(GridView.GridViewScrollViewerStyleKey);
        }

        private static void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            UpdateUnthemedScrollBarStyles();
        }

        private static void OnThemeScrollBarsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;
            if (PresentationSource.FromDependencyObject(element) != null)
                UpdateScrollBarStyles(element);
            else
                PresentationSource.AddSourceChangedHandler(element, OnPresentationSourceChanged);
        }
    }
}
