﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.TaskRunner;
using FocLauncherHost.Updater.Tasks;
using NLog;

namespace FocLauncherHost.Updater.Operations
{
    internal class CleanOperation : IUpdaterOperation
    {
        private const int ConcurrentClean = 2;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
       
        private readonly IList<CleanFileTask> _cleanFileTasks;
        private readonly AsyncTaskRunner _taskRunner;
        private readonly IList<string> _filesToBeCleaned = new List<string>();
        private readonly ConcurrentBag<string> _filesFailedToBeCleaned = new ConcurrentBag<string>();
        private bool _planSuccessful;
       

        public CleanOperation()
        {
            _taskRunner = new AsyncTaskRunner(ConcurrentClean);
            _taskRunner.Error += OnCleaningError;
            _cleanFileTasks = new List<CleanFileTask>();
        }

        private void OnCleaningError(object sender, TaskEventArgs e)
        {
            if (e.Cancel || !(e.Task is CleanFileTask task))
                return;
            _filesFailedToBeCleaned.Add(task.File);
        }

        public bool Plan()
        {
            if (_planSuccessful)
                return true;
            var files = GetFiles();

            foreach (var data in files)
            {
                var file = data.Value;
                if (!File.Exists(file)) 
                    continue;
                var cleanTask = new CleanFileTask(data.Key, file);
                _cleanFileTasks.Add(cleanTask);
                _taskRunner.Queue(cleanTask);
                _filesToBeCleaned.Add(file);
            }

            _planSuccessful = true;
            return true;
        }

        public void Run(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!Plan())
                return;
            if (!_filesToBeCleaned.Any())
                Logger.Trace("No files to clean up");
            else
            {
                Logger.Trace("These files are going to be deleted:");
                foreach (var file in _filesToBeCleaned)
                    Logger.Trace(file);

                _taskRunner.Run(token);
                try
                {
                    _taskRunner.Wait();
                }
                catch (Exception e)
                {
                }

                BackupManager.Instance.Flush();
                ComponentDownloadPathStorage.Instance.Clear();

                if (!_filesFailedToBeCleaned.Any())
                    return;
                Logger.Trace("These files have not been deleted because of an internal error:");
                foreach (var file in _filesFailedToBeCleaned)
                    Logger.Trace(file);
            }
        }

        private static IEnumerable<KeyValuePair<IComponent, string>> GetFiles()
        {
            var backupsFiles = BackupManager.Instance;
            var downloadFiles = ComponentDownloadPathStorage.Instance;
            return backupsFiles.Concat(downloadFiles).ToHashSet();
        }
    }
}
