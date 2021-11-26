using System.Collections;
using System.Collections.Generic;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater
{
    public class ComponentDownloadPathStorage : IEnumerable<KeyValuePair<IComponent, string>>
    {
        private static ComponentDownloadPathStorage? _instance;

        private readonly object _syncObject = new object();

        private readonly Dictionary<IComponent, string> _downloadLookup = new Dictionary<IComponent, string>();

        public static ComponentDownloadPathStorage Instance => _instance ??= new ComponentDownloadPathStorage();

        private ComponentDownloadPathStorage()
        {
        }

        public void Add(IComponent component, string downloadPath)
        {
            lock (_syncObject)
            {
                if (_downloadLookup.ContainsKey(component))
                    _downloadLookup[component] = downloadPath;
                else
                    _downloadLookup.Add(component, downloadPath);
            }
        }

        public bool TryGetValue(IComponent component, out string value)
        {
            lock (_syncObject)
                return _downloadLookup.TryGetValue(component, out value);
        }

        public bool Remove(IComponent component)
        {
            lock (_syncObject)
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
