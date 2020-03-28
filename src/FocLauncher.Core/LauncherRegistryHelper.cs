using Microsoft.Win32;

namespace FocLauncher
{
    public static class LauncherRegistryKeys
    {
        public const string UpdateSearchMode = nameof(UpdateSearchMode);
        public const string SessionUpdateSearchMode = nameof(SessionUpdateSearchMode);
        public const string SuppressFallbackUpdate = nameof(SuppressFallbackUpdate);
        public const string ForceRestore = nameof(ForceRestore);
    }

    public static class LauncherRegistryHelper
    {
        public const string LauncherRegistryPath = "Software\\FocLauncher";

        public static bool GetValueOrDefault<T>(string name, out T result, T defaultValue)
        {
            using var key = GetKey(true);
            var value = key.GetValue(name);
            if (!(value is null)) 
                return ValueSerializer.Deserialize(value.ToString(), out result, defaultValue);
            result = defaultValue;
            return true;
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

        public static bool GetValue<T>(string name, out T value)
        {
            return GetValueOrDefault(name, out value, default);
        }

        public static bool WriteValue(string name, object value)
        {
            try
            {
                using var key = GetKey(true);
                var serializedValue = ValueSerializer.Serialize(value, value.GetType());
                key.SetValue(name, serializedValue);
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
