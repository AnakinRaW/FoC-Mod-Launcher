using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.DPI;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;

public class ThemedImage : Image
{ 
    public static readonly DependencyProperty ActualDpiProperty = DependencyProperty.Register(nameof(ActualDpi),
        typeof(double), typeof(ThemedImage), new FrameworkPropertyMetadata(0.0));

    public static readonly DependencyProperty ActualGrayscaleBiasColorProperty =
        DependencyProperty.Register(nameof(ActualGrayscaleBiasColor), typeof(Color), typeof(ThemedImage));

    public static readonly DependencyProperty ActualHighContrastProperty =
        DependencyProperty.Register(nameof(ActualHighContrast), typeof(bool), typeof(ThemedImage));

    public static readonly DependencyProperty DpiProperty = DependencyProperty.RegisterAttached("Dpi", typeof(double),
        typeof(ThemedImage),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty GrayscaleProperty = DependencyProperty.Register(nameof(Grayscale),
        typeof(bool), typeof(ThemedImage), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty GrayscaleBiasColorProperty =
        DependencyProperty.RegisterAttached("GrayscaleBiasColor", typeof(Color?), typeof(ThemedImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty HighContrastProperty = DependencyProperty.RegisterAttached("HighContrast",
        typeof(bool?), typeof(ThemedImage),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    
    public static readonly DependencyProperty MonikerProperty = DependencyProperty.Register(nameof(Moniker),
        typeof(ImageMoniker), typeof(ThemedImage));

    public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.RegisterAttached("ScaleFactor",
        typeof(double), typeof(ThemedImage),
        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty SystemHighContrastProperty =
        DependencyProperty.Register(nameof(SystemHighContrast), typeof(bool), typeof(ThemedImage));

    public static double DefaultDpi { get; }

    public double ActualDpi => (double)GetValue(ActualDpiProperty);

    public bool ActualHighContrast => (bool)GetValue(ActualHighContrastProperty);

    public Color ActualGrayscaleBiasColor => (Color)GetValue(ActualGrayscaleBiasColorProperty);

    public bool SystemHighContrast => (bool)GetValue(SystemHighContrastProperty);
    
    public static Color? GetGrayscaleBiasColor(DependencyObject element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (Color?)element.GetValue(GrayscaleBiasColorProperty);
    }

    public static void SetGrayscaleBiasColor(DependencyObject element, Color? value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(GrayscaleBiasColorProperty, value);
    }

    public static bool? GetHighContrast(DependencyObject element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (bool?)element.GetValue(HighContrastProperty);
    }

    public static void SetHighContrast(DependencyObject element, bool? value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(HighContrastProperty, value);
    }

    public static double GetDpi(DependencyObject element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (double)element.GetValue(DpiProperty);
    }

    public static void SetDpi(DependencyObject element, double value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(DpiProperty, value);
    }

    public static double GetScaleFactor(DependencyObject element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (double)element.GetValue(ScaleFactorProperty);
    }

    public static void SetScaleFactor(DependencyObject element, double value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(ScaleFactorProperty, value);
    }

    public bool Grayscale
    {
        get => (bool)GetValue(GrayscaleProperty);
        set => SetValue(GrayscaleProperty, value);
    }

    public ImageMoniker Moniker
    {
        get => (ImageMoniker)GetValue(MonikerProperty);
        set => SetValue(MonikerProperty, value);
    }

    static ThemedImage()
    {
        try
        {
            DefaultDpi = DpiAwareness.SystemDpiX;
        }
        catch
        {
            DefaultDpi = 96.0;
        }
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedImage), new FrameworkPropertyMetadata(typeof(ThemedImage)));
    }


    public ThemedImage()
    {
        InitializeBindings();
        this.HookDpiChanged(DisplayDpiChanged);
    }

    public static ThemedImage ForMenuItem(ImageMoniker moniker, MenuItem item)
    {
        var themedImage = new ThemedImage
        {
            Moniker = moniker
        };

        themedImage.SetBinding(GrayscaleProperty, new Binding
        {
            Converter = new InverseBooleanConverter(),
            Source = item,
            Path = new PropertyPath("IsEnabled")
        });

        return themedImage;
    }

    private static void DisplayDpiChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not Visual visual)
            return;
        SetDpi(visual, visual.GetDpiX());
    }

    private void InitializeBindings()
    {
        InitializeDpiBindings();
        InitializeGrayscaleBiasColorBindings();
        InitializeHighContrastBindings();
    }

    private void InitializeDpiBindings()
    {
        BindingOperations.SetBinding(this, ActualDpiProperty, new Binding
        {
            Source = this,
            Path = new PropertyPath(DpiProperty),
            Converter = ActualDpiConverter.Instance
        });
    }

    private void InitializeGrayscaleBiasColorBindings()
    {
        BindingOperations.SetBinding(this, ActualGrayscaleBiasColorProperty, new MultiBinding
        {
            Bindings =
            {
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(GrayscaleBiasColorProperty)
                },
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(ActualHighContrastProperty)
                }
            },
            Converter = ActualGrayscaleBiasColorConverter.Instance
        });
    }

    private void InitializeHighContrastBindings()
    {
        SetResourceReference(SystemHighContrastProperty, SystemParameters.HighContrastKey);
        BindingOperations.SetBinding(this, ActualHighContrastProperty, new MultiBinding
        {
            Bindings = {
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(HighContrastProperty)
                },
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(SystemHighContrastProperty)
                }
            },
            Converter = ActualHighContrastConverter.Instance
        });
    }
}