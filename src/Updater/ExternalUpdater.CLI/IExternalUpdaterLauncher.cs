using System.Diagnostics;
using System.IO.Abstractions;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.ExternalUpdater.CLI;

public interface IExternalUpdaterLauncher
{
    Process Start(IFileInfo updater, ExternalUpdaterArguments arguments);
}