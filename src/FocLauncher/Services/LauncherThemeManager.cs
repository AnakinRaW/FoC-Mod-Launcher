using System;
using Sklavenwalker.CommonUtilities.Wpf.Services;

namespace FocLauncher.Services;

internal sealed class LauncherThemeManager : ThemeManager
{
    public LauncherThemeManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}