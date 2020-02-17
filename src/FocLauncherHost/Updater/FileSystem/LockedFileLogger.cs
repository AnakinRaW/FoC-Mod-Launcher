using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;

namespace FocLauncherHost.Updater.FileSystem
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
                //var processes = LockingProcessMaanager.Instance.GetProcesses(files);
                //if (!processes.IsNullOrEmpt())
                //    Logger.Warn($"The following file(s) are locked: {files.TryJoin(", ")}, process(es): {processes.Select(Format).TryJoin(", ")}");
            }
            var dirs = paths.Where(Directory.Exists).ToArray();
            if (dirs.Length == 0)
                return;
            //var viewer = (services1 != null ? services1.GetService<IHandleViewer>(false) : (IHandleViewer)null) ?? (IHandleViewer)new HandleViewer(this.services);
            //var source1 = array2.SelectMany((directory => viewer.GetHandleInformation(directory))).Distinct();
            //if (source1.IsNullOrEmpty())
            //    return;
            //var source2 = source1.Select(Format);
            //Logger.Warn($"The following directory/directories are locked: {array2.TryJoin(", ")}, process(es): {source2.TryJoin(", ")}");
        }

        //private string Format(ILockingProcessInfo info)
        //{
        //    return "";
        //}

    }
}