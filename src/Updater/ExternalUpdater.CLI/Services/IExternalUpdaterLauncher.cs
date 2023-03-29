using System.Diagnostics;
using System.IO.Abstractions;
using AnakinRaW.ExternalUpdater.Options;

namespace AnakinRaW.ExternalUpdater.Services;

public interface IExternalUpdaterLauncher
{
    Process Start(IFileInfo updater, ExternalUpdaterOptions options);
}