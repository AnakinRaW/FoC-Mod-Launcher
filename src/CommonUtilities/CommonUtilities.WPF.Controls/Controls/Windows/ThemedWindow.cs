using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedWindow : ShadowChromeWindow
{
    static ThemedWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedWindow), new FrameworkPropertyMetadata(typeof(ThemedWindow)));
    }
    
    public ThemedWindow(IWindowViewModel dataContext) : base(dataContext)
    {
    }
}