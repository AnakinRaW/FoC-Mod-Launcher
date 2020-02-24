using System;
using System.IO;
using System.Threading;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.Download
{
    internal interface IDownloadEngine
    {
        string Name { get; }

        bool IsSupported(DownloadSource source);

        DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback progress, 
            CancellationToken cancellationToken, IComponent? component);
    }
}