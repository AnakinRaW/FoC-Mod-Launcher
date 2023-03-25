using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Elevation;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ExternalUpdater.CLI;

public class ExternalUpdaterLauncher : IExternalUpdaterLauncher
{
    private readonly IProcessElevation _elevation;

    public ExternalUpdaterLauncher(IServiceProvider serviceProvider)
    {
        _elevation = serviceProvider.GetRequiredService<IProcessElevation>();
    }

    public Process Start(IFileInfo updater, ExternalUpdaterArguments arguments)
    {
        Requires.NotNull(updater, nameof(updater));
        Requires.NotNull(arguments, nameof(arguments));

        if (!updater.Exists)
            throw new FileNotFoundException("Could not find updater application", updater.FullName);
        
        var startInfo = CreateStartInfo(updater.FullName, arguments);
        return Process.Start(startInfo)!;
    }


    private ProcessStartInfo CreateStartInfo(string updater, ExternalUpdaterArguments arguments)
    {

        var externalUpdateStartInfo = new ProcessStartInfo(updater)
        {
#if !DEBUG
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
        };

        if (_elevation.IsElevated)
            externalUpdateStartInfo.Verb = "runas";

        externalUpdateStartInfo.Arguments = arguments.ToCommandLine();
        return externalUpdateStartInfo;
    }
}