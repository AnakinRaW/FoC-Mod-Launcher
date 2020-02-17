﻿namespace FocLauncherHost.Updater.FileSystem
{
    internal class OutOfDiskspaceException : UpdaterException
    {
        public OutOfDiskspaceException() : base(nameof(OutOfDiskspaceException))
        {
        }

        public OutOfDiskspaceException(string message) : base(message)
        {
        }
    }
}