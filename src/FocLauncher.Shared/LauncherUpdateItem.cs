using Newtonsoft.Json;

namespace FocLauncher.Shared
{
    public class LauncherUpdaterItem
    {
        public string File { get; set; }

        public string? Destination { get; set; }

        public string? Backup { get; set; }

        public override string ToString()
        {
            return $"LauncherUpdaterItem - File: '{File}'; Destination: '{Destination}'; Backup: {Backup}";
        }
    }
}
