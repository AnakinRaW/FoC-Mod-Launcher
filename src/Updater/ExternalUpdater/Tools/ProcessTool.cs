using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.CLI;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ExternalUpdater.Tools;

internal abstract class ProcessTool<T> : ToolBase<T> where T : ExternalUpdaterArguments
{
    protected ProcessTool(T arguments, IServiceProvider serviceProvider) : base(arguments, serviceProvider)
    {
    }

    protected async Task WaitForProcessExitAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Arguments.Timeout));
        if (!await ProcessTools.WaitForExitAsync(Arguments.Pid, cts.Token))
            throw new InvalidOperationException($"Application with '{Arguments.Pid}' was not closed");
    }

    protected void StartProcess(ExternalUpdaterResult appStartOptions)
    {
        var processToStart = FileSystem.FileInfo.New(Arguments.AppToStart);
        if (!processToStart.Exists)
            throw new FileNotFoundException("Could not find application to restart.", processToStart.FullName);

        Logger?.LogInformation($"Starting application '{processToStart.FullName}'");
        ProcessTools.StartApplication(processToStart, appStartOptions, Arguments.Elevate);
    }
}