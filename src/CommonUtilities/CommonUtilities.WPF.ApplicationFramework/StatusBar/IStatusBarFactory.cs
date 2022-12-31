﻿using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IStatusBarFactory
{
    FrameworkElement CreateStatusBar(IStatusBarViewModel viewModel);
}