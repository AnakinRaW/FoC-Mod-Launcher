using Microsoft.Win32;

namespace FocLauncher
{
    public static class LauncherRegistryKeys
    {
        public const string UpdateSearchMode = nameof(UpdateSearchMode);
        public const string SessionUpdateSearchMode = nameof(SessionUpdateSearchMode);
    }

    public static class LauncherRegistryHelper
    {
        public const string LauncherRegistryPath = "Software\\FocLauncher";

        public static T GetValueOrDefault<T>(string name, T defaultValue)
        {
            using var key = GetKey(true);
            return (T) key.GetValue(name, defaultValue);
        }

        public static T GetValueOrSetDefault<T>(string name, T defaultValue)
        {
            using var key = GetKey(true);
            var value = key.GetValue(name);
            if (value == null)
            {
                WriteValue(name, defaultValue);
                return defaultValue;
            }

            return (T) value;
        }

        public static T GetValue<T>(string name)
        {
            using var key = GetKey();
            return (T) key.GetValue(name);
        }

        public static bool WriteValue(string name, object value)
        {
            try
            {
                using var key = GetKey(true);
                key.SetValue(name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteValue(string name)
        {
            try
            {
                using var key = GetKey(true);
                key.DeleteValue(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Initialize()
        {
            Registry.CurrentUser.CreateSubKey(LauncherRegistryPath)?.Dispose();
        }

        private static RegistryKey GetKey(bool writable = false)
        {
            return Registry.CurrentUser.OpenSubKey(LauncherRegistryPath, writable);
        }
    }
}
