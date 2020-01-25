using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FocLauncher
{
    public static class AppDomainHelper
    {
        public static Assembly LoadLauncherAssemblyFrom(object sender, ResolveEventArgs args)
        {
            var fields = args.Name.Split(',');
            var name = fields[0];
            var culture = fields[2];

            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;

            var files = Directory.EnumerateFiles(LauncherConstants.ApplicationBasePath, "*.dll", SearchOption.TopDirectoryOnly);
            var dll = files.FirstOrDefault(x => $"{name}.dll".Equals(Path.GetFileName(x)));
            return dll == null ? null : Assembly.LoadFile(dll);
        }
    }
}
