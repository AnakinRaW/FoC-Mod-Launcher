﻿using TaskBasedUpdater.Restart;

// ReSharper disable once CheckNamespace
namespace FocLauncher.Shared
{
    // This is by far the nastiest hack I've ever done, but thanks to the way .NET is build it works like magic.
    public partial class LauncherRestartOptions : IRestartOptions
    {
    }
}