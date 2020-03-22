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

namespace FocLauncher.AppUpdater
{
    internal static class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        internal static void Main(string[] args)
        {
            try
            {
                var updateResult = ExternalUpdaterResult.NoUpdate;

                var parserResult = Parser.Default.ParseArguments<LauncherRestartOptions>(args);
                parserResult.WithParsed(o =>
                {
                    if (o.Pid.HasValue)
                    {
                        var parentProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == o.Pid.Value);
                        if (parentProcess != null)
                        {
                            try
                            {
                                Console.WriteLine($"Waiting for {parentProcess.ProcessName} to exit...");
                                if (!WaitForExitAsync(parentProcess, o.Timeout * 1000, CancellationTokenSource.Token)
                                    .Result)
                                {
                                    Console.WriteLine(
                                        $"The process '{parentProcess.ProcessName}:{parentProcess.Id}' did not exit within 10 seconds or the user requested cancellation. Aborting elevation.");
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(
                                    $"Unable to wait for process '{parentProcess.ProcessName}:{parentProcess.Id}' to terminate: {e.Message}");
                                throw;
                            }
                        }
                    }

                    if (o.Update)
                    {
                        Console.WriteLine("Updating...");
                        try
                        {
                            Console.WriteLine("Deserializing Payload");
                            var updateItems = JsonConvert.DeserializeObject<List<LauncherUpdaterItem>>(Base64Decode(o.Payload));
                            Console.WriteLine("Payload Deserialized");
                            var updater = new ExternalUpdater(updateItems);
                            updateResult = updater.Run();
                            Console.WriteLine($"Updated with result: {updateResult}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }

                    var launcher = o.ExecutablePath;
                    if (!File.Exists(launcher))
                        throw new FileNotFoundException("The launcher executable was not found.", launcher);

                    var launcherStartInfo = new ProcessStartInfo(launcher) {Arguments = updateResult.ToString()};
                    using var process = new Process {StartInfo = launcherStartInfo};

                    Console.WriteLine($"Starting {launcher}");
#if DEBUG
                    Console.ReadKey();
#endif
                    process.Start();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
            finally
            {
                Environment.Exit(0);
            }
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
