namespace FocLauncher.Shared
{
    internal class LauncherUpdaterItem
    {
        public string? File { get; set; }

        public string? Destination { get; set; }

        public string? Backup { get; set; }

        public string? BackupDestination { get; set; }

        public override string ToString()
        {
            return $"LauncherUpdaterItem - File: '{File}'; Destination: '{Destination}'; Backup: {Backup}; Backup Destination: {BackupDestination}";
        }
    }
}
