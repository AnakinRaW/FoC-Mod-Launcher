using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FocLauncher.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AnakinRaW.CommonUtilities;

namespace FocLauncher.AppUpdater;

internal class Application
{
    private readonly LauncherUpdaterOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;

    public Application(LauncherUpdaterOptions options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
    }

    public async Task<ExternalUpdaterResult> Run()
    {
        if (!await WaitForProcessToClose())
            return ExternalUpdaterResult.NoUpdate;

        var updateResult = ExternalUpdaterResult.NoUpdate;
        if (_options.Restore)
            updateResult = ExternalUpdaterResult.DemandsRestore;

        if (_options.Update)
        {
            _logger?.LogInformation("Updating...");
            try
            {
                _logger?.LogDebug("Deserializing Payload");
                var updateItems = JsonConvert.DeserializeObject<List<LauncherUpdateItem>>(Base64Decode(_options.Payload));
                _logger?.LogDebug("Payload Deserialized");
                var updater = new ExternalUpdater(updateItems!, _serviceProvider);
                updateResult = updater.Run();
                _logger?.LogDebug($"Updated with result: {updateResult}");
            }
            catch (Exception e)
            {
                _logger?.LogCritical(e, e.Message);
                throw;
            }
        }

        var launcher = _options.ExecutablePath;
        if (!File.Exists(launcher))
            throw new FileNotFoundException("The launcher executable was not found.", launcher);

        var launcherStartInfo = new ProcessStartInfo(launcher) { Arguments = ((int)updateResult).ToString() };
        using var process = new Process { StartInfo = launcherStartInfo };

        _logger?.LogInformation($"Starting {launcher}");
        process.Start();

        return 0;
    }

    private async Task<bool> WaitForProcessToClose()
    {
        if (_options.Pid.HasValue)
        {
            var parentProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == _options.Pid.Value);
            if (parentProcess != null)
            {
                try
                {
                    _logger?.LogDebug($"Waiting for {parentProcess.ProcessName} to exit...");
                    var cts = new CancellationTokenSource(_options.Timeout * 1000);
                    await parentProcess.WaitForExitAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger?.LogError($"The process '{parentProcess.ProcessName}:{parentProcess.Id}' did not exit after {_options.Timeout} seconds. Aborting...");
                    return false;
                }
                catch (Exception e)
                {
                    _logger?.LogCritical(e, $"Unable to wait for process '{parentProcess.ProcessName}:{parentProcess.Id}' to terminate: {e.Message}");
                    return false;
                }
            }
        }
        return true;
    }

    private static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}