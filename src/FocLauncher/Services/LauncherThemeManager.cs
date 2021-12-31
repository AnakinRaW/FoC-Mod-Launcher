using System;
using Sklavenwalker.CommonUtilities.Wpf.Services;
using Sklavenwalker.CommonUtilities.Wpf.Theming;

namespace FocLauncher.Services;

internal sealed class LauncherThemeManager : ThemeManager
{
    public LauncherThemeManager(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}