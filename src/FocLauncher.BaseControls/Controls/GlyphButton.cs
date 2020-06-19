using System.Windows;
using System.Windows.Media;

namespace FocLauncher.Controls
{
    internal class GlyphButton : RoutedCommandButton
    {
        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(nameof(PressedBorderBrush), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty PressedBorderThicknessProperty = DependencyProperty.Register(nameof(PressedBorderThickness), typeof(Thickness), typeof(GlyphButton));
        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty HoverBorderBrushProperty = DependencyProperty.Register(nameof(HoverBorderBrush), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty HoverBorderThicknessProperty = DependencyProperty.Register(nameof(HoverBorderThickness), typeof(Thickness), typeof(GlyphButton));
        public static readonly DependencyProperty GlyphForegroundProperty = DependencyProperty.Register(nameof(GlyphForeground), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register(nameof(HoverForeground), typeof(Brush), typeof(GlyphButton));
        public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(nameof(PressedForeground), typeof(Brush), typeof(GlyphButton));

        static GlyphButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GlyphButton), new FrameworkPropertyMetadata(typeof(GlyphButton)));
        }

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public Brush PressedBorderBrush
        {
            get => (Brush)GetValue(PressedBorderBrushProperty);
            set => SetValue(PressedBorderBrushProperty, value);
        }

        public Thickness PressedBorderThickness
        {
            get => (Thickness)GetValue(PressedBorderThicknessProperty);
            set => SetValue(PressedBorderThicknessProperty, value);
        }

        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public Brush HoverBorderBrush
        {
            get => (Brush)GetValue(HoverBorderBrushProperty);
            set => SetValue(HoverBorderBrushProperty, value);
        }

        public Thickness HoverBorderThickness
        {
            get => (Thickness)GetValue(HoverBorderThicknessProperty);
            set => SetValue(HoverBorderThicknessProperty, value);
        }

        public Brush GlyphForeground
        {
            get => (Brush)GetValue(GlyphForegroundProperty);
            set => SetValue(GlyphForegroundProperty, value);
        }

        public Brush HoverForeground
        {
            get => (Brush)GetValue(HoverForegroundProperty);
            set => SetValue(HoverForegroundProperty, value);
        }

        public Brush PressedForeground
        {
            get => (Brush)GetValue(PressedForegroundProperty);
            set => SetValue(PressedForegroundProperty, value);
        }
    }
}