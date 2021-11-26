using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TaskBasedUpdater.Elevator
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            try
            {
                if (args.Length <= 1)
                {
                    Console.WriteLine("Usage: process_executable_file [arguments]");
                    return;
                }


                var arguments = string.Empty;
                if (args.Length > 1)
                    arguments = string.Join(" ", args.Skip(1));

                Console.WriteLine(arguments);

                var processFile = args[0];
                if (!File.Exists(processFile))
                {
                    Console.WriteLine($"File '{processFile}' does not exists.");
                    Console.WriteLine("Usage: process_executable_file [arguments]");
                    return;
                }

                var startInfo = new ProcessStartInfo(processFile) {Verb = "runas", UseShellExecute = true};

                if (!string.IsNullOrEmpty(arguments))
                {
                    startInfo.Arguments = arguments;
                    Console.WriteLine($"Added arguments: {startInfo.Arguments}");
                }

                using var process = new Process {StartInfo = startInfo};
                Console.WriteLine($"Starting process {process.StartInfo.FileName}");
                process.Start();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
