using CommandLine;

namespace AnakinRaW.ExternalUpdater.CLI.Arguments;

[Verb("restart", HelpText = "Waits for a given process to close and starts a new given process (usually the one that was just closed).")]
public record RestartArguments : ExternalUpdaterArguments
{
}

[Verb("update", HelpText = "Updates the given application and restarts it")]
public record UpdateArguments : ExternalUpdaterArguments
{
}




public record ExternalUpdaterArguments
{
    [Option('s', "startProcess", Required = true, HelpText = "The absolute path of the application to start.")]
    public required string AppToStart { get; init; }

    [Option('e', "elevate", Required = false, HelpText = "The application shall be started with higher rights.")]
    public bool Elevate { get; init; }

    [Option('t', "timeout", Required = false, HelpText = "The maximum time in seconds to wait for the specified process to terminate.", Default = 10)]
    public int Timeout { get; init; }

    [Option('p', "pid", Required = false, HelpText = "The PID of the process to wait until terminated.")]
    public int? Pid { get; init; }

    [Option('l', "logfile", Required = false, HelpText = "The absolute path of the log file to use.")]
    public string? LogFile { get; init; }

    public string ToCommandLine()
    {
        return Parser.Default.FormatCommandLine(this, config => config.SkipDefault = true);
    }
}

public static class ExternalUpdaterArgumentUtilities
{
    public static ExternalUpdaterArguments FromString(string commandLineString)
    {
        return null;
    }

    public static ExternalUpdaterArguments WithCurrentData(this ExternalUpdaterArguments arguments)
    {
        return arguments;
    }
}