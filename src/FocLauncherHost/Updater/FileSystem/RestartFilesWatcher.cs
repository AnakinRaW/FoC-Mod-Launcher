using System;
using System.Collections.Generic;

namespace FocLauncherHost.Updater.FileSystem
{
    internal class RestartFilesWatcher
    {
        private static RestartFilesWatcher _instance;

        private readonly HashSet<string> _filesToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static RestartFilesWatcher Instance => _instance ??= new RestartFilesWatcher();

        public ISet<string> FilesToBeDeleted => _filesToDelete;

        private RestartFilesWatcher()
        {
        }

        public void Clear()
        {
            this._filesToDelete.Clear();
        }
    }
}