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
    public bool Elevate { get; init; }

    public int Timeout { get; init; }

    public int? Pid { get; init; }

    public required string AppToStart { get; init; }

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