using System;
using System.Diagnostics;

namespace TaskBasedUpdater
{
   internal static class ProcessUtilities
    {
        public static CurrentProcessInfo GetCurrentProcessInfo()
        {
            var process = Process.GetCurrentProcess();
          
            var result = new CurrentProcessInfo { Id = process.Id, Arguments = Environment.CommandLine};
            return result;
        }

        internal struct CurrentProcessInfo
        {
            public int Id;
            public string Arguments;
        }
    }
}
