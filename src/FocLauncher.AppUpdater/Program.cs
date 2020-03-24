using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using FocLauncher.Shared;
using Newtonsoft.Json;
using NLog;
using NLog.Conditions;
using NLog.Targets;

namespace FocLauncher.AppUpdater
{
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        internal static void Main(string[] args)
        {
            try
            {
                var updateResult = ExternalUpdaterResult.NoUpdate;

                var parserResult = Parser.Default.ParseArguments<LauncherRestartOptions>(args);
                parserResult.WithParsed(o =>
                {
                    SetLogging(o.LogFile);
                    if (o.Pid.HasValue)
                    {
                        var parentProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == o.Pid.Value);
                        if (parentProcess != null)
                        {
                            try
                            {
                                Logger.Debug($"Waiting for {parentProcess.ProcessName} to exit...");
                                if (!WaitForExitAsync(parentProcess, o.Timeout * 1000, CancellationTokenSource.Token)
                                    .Result)
                                {
                                    Logger.Error(
                                        $"The process '{parentProcess.ProcessName}:{parentProcess.Id}' did not exit within 10 seconds or the user requested cancellation. Aborting elevation.");
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Fatal(e, $"Unable to wait for process '{parentProcess.ProcessName}:{parentProcess.Id}' to terminate: {e.Message}");
                                throw;
                            }
                        }
                    }

                    if (o.Update)
                    {
                        Logger.Info("Updating...");
                        try
                        {
                            Logger.Debug("Deserializing Payload");
                            var updateItems = JsonConvert.DeserializeObject<List<LauncherUpdaterItem>>(Base64Decode(o.Payload));
                            Logger.Debug("Payload Deserialized");
                            var updater = new ExternalUpdater(updateItems);
                            updateResult = updater.Run();
                            Logger.Debug($"Updated with result: {updateResult}");
                        }
                        catch (Exception e)
                        {
                            Logger.Fatal(e);
                            throw;
                        }
                    }

                    var launcher = o.ExecutablePath;
                    if (!File.Exists(launcher))
                        throw new FileNotFoundException("The launcher executable was not found.", launcher);

                    var launcherStartInfo = new ProcessStartInfo(launcher) {Arguments = ((int) updateResult).ToString()};
                    using var process = new Process {StartInfo = launcherStartInfo};

                    Logger.Info($"Starting {launcher}");
                    process.Start();
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
#if DEBUG
                Console.WriteLine("Press enter to close!");
                Console.ReadKey();
#endif
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void SetLogging(string? logfile)
        {
            var config = new NLog.Config.LoggingConfiguration();

            Target logfileTarget = null;
            if (!string.IsNullOrEmpty(logfile) && File.Exists(logfile))
                logfileTarget = new FileTarget("logfile") { FileName = logfile };
#if DEBUG
            if (logfileTarget != null)
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfileTarget);
            var consoleTarget = new ColoredConsoleTarget();
            var highlightRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Info"),
                ForegroundColor = ConsoleOutputColor.Green
            };
            consoleTarget.RowHighlightingRules.Add(highlightRule);
            
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);
#else
            if (logfileTarget != null)
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logfileTarget);
#endif
            LogManager.Configuration = config;
        }

        private static async Task<bool> WaitForExitAsync(Process process, int timeout, CancellationToken token)
        {
            var processTask = Task.Run(() => process.WaitForExit(timeout), token);
            return await Task.WhenAny(Task.Delay(timeout, token), processTask) == processTask && processTask.Result;
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
