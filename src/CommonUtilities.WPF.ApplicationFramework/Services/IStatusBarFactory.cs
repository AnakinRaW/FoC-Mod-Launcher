using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Services;

public interface IStatusBarFactory
{
    UIElement CreateStatusBar();
}