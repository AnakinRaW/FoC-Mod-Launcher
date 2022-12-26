using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal class WindowTitleBarButton : GlyphButton
{
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius),
        typeof(CornerRadius), typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(new CornerRadius(0.0)));

    public static readonly DependencyProperty HitTestResultProperty = DependencyProperty.Register(nameof(HitTestResult),
        typeof(int), typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(1));

    public static readonly DependencyProperty IsNCMouseOverProperty = DependencyProperty.Register(nameof(IsNCMouseOver),
        typeof(bool), typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsNCPressedProperty = DependencyProperty.Register(nameof(IsNCPressed),
        typeof(bool), typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(false));

    private static readonly BrushToColorConverter BrushToColorConverter = new();

    private Border? _border;

    static WindowTitleBarButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(typeof(WindowTitleBarButton)));
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public int HitTestResult
    {
        get => (int)GetValue(HitTestResultProperty);
        set => SetValue(HitTestResultProperty, value);
    }

    public bool IsNCMouseOver
    {
        get => (bool)GetValue(IsNCMouseOverProperty);
        set => SetValue(IsNCMouseOverProperty, value);
    }

    public bool IsNCPressed
    {
        get => (bool)GetValue(IsNCPressedProperty);
        set => SetValue(IsNCPressedProperty, value);
    }

    public WindowTitleBarButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => SubscribeToBorderBackgroundChanges();

    private void OnUnloaded(object sender, RoutedEventArgs e) => UnsubscribeFromBorderBackgroundChanges();

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        SubscribeToBorderBackgroundChanges();
    }

    private void SubscribeToBorderBackgroundChanges()
    {
        UnsubscribeFromBorderBackgroundChanges();
        _border = GetTemplateChild("PART_Border") as Border;
        if (_border == null)
            return;
        _border.AddPropertyChangeHandler(Border.BackgroundProperty, OnBorderBackgroundChanged);
        OnBorderBackgroundChanged(_border, EventArgs.Empty);
    }

    private void UnsubscribeFromBorderBackgroundChanges()
    {
        var border = _border;
        border?.RemovePropertyChangeHandler(Border.BackgroundProperty, OnBorderBackgroundChanged);
    }

    private void OnBorderBackgroundChanged(object sender, EventArgs e)
    {
        if (sender is not Border border)
            return;
        var color = (Color)BrushToColorConverter.Convert(border.Background, typeof(Color), null, CultureInfo.CurrentCulture)!;
        if (color.A > 127)
            SetValue(ImageThemingUtilities.ImageBackgroundColorProperty, color);
        else
            ClearValue(ImageThemingUtilities.ImageBackgroundColorProperty);
    }

    protected override int HitTestCore(Point point) => HitTestResult;
}