using System;
using Flurl;

namespace FocLauncher;

internal static class LauncherConstants
{
    public const string ApplicationName = "Foc Mod Launcher";

    public static readonly Url LauncherRootUrl = new(new Uri("https://republicatwar.com/downloads/FocLauncher/v2"));
}