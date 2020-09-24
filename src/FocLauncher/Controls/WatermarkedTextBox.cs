using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    public class WatermarkedTextBox : TextBox
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(WatermarkedTextBox));

        static WatermarkedTextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkedTextBox), new FrameworkPropertyMetadata(typeof(WatermarkedTextBox)));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, (object)value);
        }
    }
}
