using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedUpdater.Elevator
{
    internal class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        internal static void Main(string[] args)
        {
            Console.CancelKeyPress += OnCancel;

            try
            {
                if (args.Length <= 1)
                {
                    Console.WriteLine("parent_process process_executable_file [arguments]");
                    return;
                }

                if (int.TryParse(args[0], out var parentProcessPid))
                {
                    var parentProcess = Process.GetProcesses().FirstOrDefault(x => x.Id == parentProcessPid);
                    if (parentProcess != null)
                    {
                        if (!WaitForExitAsync(parentProcess, 10000, CancellationTokenSource.Token).Result)
                        {
                            Console.WriteLine(
                                "The parent process did not exit within 10 seconds or the user requested cancellation. Aborting elevation.");
                            return;
                        }
                    }

                }

                var processFile = args[1];
                if (!File.Exists(processFile))
                {
                    Console.WriteLine($"File '{processFile}' does not exists.");
                    return;
                }

                var startInfo = new ProcessStartInfo(processFile) {Verb = "runas", UseShellExecute = true};

                if (args.Length == 3)
                {
                    startInfo.Arguments = args[2];
                    Console.WriteLine($"Added arguments: {startInfo.Arguments}");
                }

                var process = new Process {StartInfo = startInfo};
                using (process)
                {
                    Console.WriteLine($"Starting process {process.StartInfo.FileName}");
                    process.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }

        private static void OnCancel(object sender, ConsoleCancelEventArgs e)
        {
            CancellationTokenSource.Cancel();
            e.Cancel = true;
        }

        private static async Task<bool> WaitForExitAsync(Process process, int timeout, CancellationToken token)
        {
            var processTask = Task.Run(() => process.WaitForExit(timeout), token);
            return await Task.WhenAny(Task.Delay(timeout, token), processTask) == processTask && processTask.Result;
        }
    }
}
