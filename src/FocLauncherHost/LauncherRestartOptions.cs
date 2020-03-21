using CommandLine;
using TaskBasedUpdater.Restart;

namespace FocLauncherHost
{
    public class LauncherRestartOptions : IUpdateOptions
    {
        [Option('p', "pid", Required = false, HelpText = "The PID of the process to wait until terminated.")]
        public int? Pid { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "The maximum time in seconds to wait for the specified process to terminate.", Default = 10)]
        public int Timeout { get; set; }

        [Option('l', "location", Required = true, HelpText = "The absolute path of the Foc Launcher.exe file.")]
        public string ExecutablePath { get; set; }

        [Option('u', "update", Required = false, HelpText = "Tell the application to run the update procedure.")]
        public bool Update { get; set; }

        public string Unparse()
        {
            return Parser.Default.FormatCommandLine(this, config => config.SkipDefault = true);
        }
    }
}