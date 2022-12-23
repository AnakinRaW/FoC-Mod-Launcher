using System;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Utils;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedWindow : ShadowChromeWindow
{
    static ThemedWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedWindow), new FrameworkPropertyMetadata(typeof(ThemedWindow)));
    }
    
    public ThemedWindow(IWindowViewModel dataContext) : base(dataContext)
    {
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetWindowIcon();
    }

    protected T? GetTemplateChild<T>(string name) where T : class
    {
        var templateChild = GetTemplateChild(name) as T;
        try
        {
        }
        catch (InvalidOperationException)
        {
        }
        return templateChild;
    }

    private void SetWindowIcon()
    {
        IconHelper.UseWindowIconAsync(windowIcon => Icon = windowIcon);
    }
}