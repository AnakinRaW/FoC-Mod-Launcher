﻿using System;
using System.IO;
using System.Threading;

namespace FocLauncherHost.Updater.Download
{
    internal class FileDownloader : DownloadEngineBase
    {
        public FileDownloader() : base("File", new DownloadSource[1])
        {
        }

        protected override DownloadSummary DownloadCore(Uri uri, Stream outputStream, ProgressUpdateCallback progress,
            CancellationToken cancellationToken, DownloadContext downloadContext)
        {
            if (!uri.IsFile && !uri.IsUnc)
                throw new ArgumentException("Expected file or UNC path", nameof(uri));
            return new DownloadSummary
            {
                DownloadedSize = UpdaterUtilities.CopyFileToStream(uri.LocalPath, outputStream, progress, cancellationToken)
            };
        }
    }
}