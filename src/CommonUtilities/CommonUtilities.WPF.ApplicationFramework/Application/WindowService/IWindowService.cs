﻿using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public interface IWindowService
{
    void SetMainWindow(Window window);

    void ShowWindow();

    void SetOwner(Window window);

    void DisableOwner(Window window);

    void EnableOwner(Window window);
}