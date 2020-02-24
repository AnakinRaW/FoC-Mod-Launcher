using System;
using System.IO;
using System.Threading;

namespace FocLauncherHost.Updater.Download
{
    internal interface IDownloadEngine
    {
        string Name { get; }

        bool IsSupported(DownloadSource source);

        DownloadSummary Download(Uri uri, Stream outputStream, ProgressUpdateCallback progress, 
            CancellationToken cancellationToken, DownloadContext downloadContext);
    }
}