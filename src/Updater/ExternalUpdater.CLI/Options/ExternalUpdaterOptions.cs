using CommandLine;

namespace AnakinRaW.ExternalUpdater.Options;

public record ExternalUpdaterOptions
{
    [Option('s', "startProcess", Required = true, HelpText = "The absolute path of the application to start.")]
    public required string AppToStart { get; init; }

    [Option('e', "elevate", Required = false, HelpText = "The application shall be started with higher rights.")]
    public bool Elevate { get; init; }

    [Option('t', "timeout", Required = false, HelpText = "The maximum time in seconds to wait for the specified process to terminate.", Default = 10)]
    public int Timeout { get; init; }

    [Option('p', "pid", Required = false, HelpText = "The PID of the process to wait until terminated.")]
    public int? Pid { get; init; }
}