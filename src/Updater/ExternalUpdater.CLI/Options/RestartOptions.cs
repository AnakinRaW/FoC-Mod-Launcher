using CommandLine;

namespace AnakinRaW.ExternalUpdater.Options;

[Verb("restart", HelpText = "Waits for a given process to close and starts a new given process (usually the one that was just closed).")]
public sealed record RestartOptions : ExternalUpdaterOptions;