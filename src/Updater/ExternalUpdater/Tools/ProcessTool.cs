using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.Options;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ExternalUpdater.Tools;

internal abstract class ProcessTool<T> : ToolBase<T> where T : ExternalUpdaterOptions
{
    protected ProcessTool(T options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    protected async Task WaitForProcessExitAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Options.Timeout));
        if (!await ProcessTools.WaitForExitAsync(Options.Pid, cts.Token))
            throw new InvalidOperationException($"Application with '{Options.Pid}' was not closed");
    }

    protected void StartProcess(ExternalUpdaterResult appStartOptions)
    {
        var processToStart = FileSystem.FileInfo.New(Options.AppToStart);
        if (!processToStart.Exists)
            throw new FileNotFoundException("Could not find application to restart.", processToStart.FullName);

        Logger?.LogInformation($"Starting application '{processToStart.FullName}'");
        ProcessTools.StartApplication(processToStart, appStartOptions, Options.Elevate);
    }
}