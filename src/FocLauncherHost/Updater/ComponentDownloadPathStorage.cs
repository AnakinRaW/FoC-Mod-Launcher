using System.Collections;
using System.Collections.Generic;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater
{
    internal class ComponentDownloadPathStorage : IEnumerable<KeyValuePair<IComponent, string>>
    {
        private static ComponentDownloadPathStorage _instance;

        private readonly Dictionary<IComponent, string> _downloadLookup = new Dictionary<IComponent, string>();

        public static ComponentDownloadPathStorage Instance => _instance ??= new ComponentDownloadPathStorage();

        private ComponentDownloadPathStorage()
        {
        }

        public void Add(IComponent component, string downloadPath)
        {
            if (_downloadLookup.ContainsKey(component))
                _downloadLookup[component] = downloadPath;
            else
                _downloadLookup.Add(component, downloadPath);
        }

        public bool TryGetValue(IComponent component, out string value)
        {
            return _downloadLookup.TryGetValue(component, out value);
        }

        public bool Remove(IComponent component)
        {
            return _downloadLookup.Remove(component);
        }

        public void Clear()
        {
            _downloadLookup.Clear();
        }

        public IEnumerator<KeyValuePair<IComponent, string>> GetEnumerator()
        {
            return _downloadLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
