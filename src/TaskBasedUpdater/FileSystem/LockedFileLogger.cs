using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater.FileSystem
{
    internal class LockedFileLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static LockedFileLogger _instance;

        public static LockedFileLogger Instance => _instance ??= new LockedFileLogger();

        private LockedFileLogger()
        {
        }

        public void Log(IEnumerable<string> paths)
        {
            if (paths == null || Logger == null)
                return;
            var files = paths.Where(File.Exists).ToArray();
            if (files.Length != 0)
            {
                var processes = LockingProcessManager.GetProcesses(files);
                if (!processes.IsNullOrEmpty())
                    Logger.Warn($"The following file(s) are locked: {files.TryJoin(", ")}, process(es): {processes.Select(Format).TryJoin(", ")}");
            }
            // TODO: Directories??
        }

        private static string Format(ILockingProcessInfo info)
        {
            if (info == null)
                return (string)null;
            var str = info.Description;
            if (info.ApplicationType == ApplicationType.Service)
                str = info.ServiceName;
            else
            {
                var process = Process.GetProcessById(info.Id);
                if (process.StartTime == info.StartTime)
                    str = process.ProcessName;
            }
            return $"{str} ({info.Id})";
        }

    }
}