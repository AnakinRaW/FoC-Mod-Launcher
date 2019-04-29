using System.Collections.Generic;

namespace FocLauncher.Core
{
    public static class KnownModIds
    {
        private static readonly Dictionary<string, string> _knownKeys = new Dictionary<string, string>();

        static KnownModIds()
        {
            _knownKeys.Add("1125571106", "Thrawn's Revenge: Imperial Civil War");
            _knownKeys.Add("1129810972", "Republic at War");
            _knownKeys.Add("1235783994", "Phoenix Rising");
            _knownKeys.Add("1397421866", "Awakening of the Rebellion");
            _knownKeys.Add("1126673817", "The Clone Wars");
            _knownKeys.Add("1125764259", "Star Wars Battlefront Commander");
            _knownKeys.Add("1172650432", "Yodenmod2018");
            _knownKeys.Add("1130150761", "Old Republic at War (Classic)");
            _knownKeys.Add("1193084624", "Awakening of the Rebellion 2.6");
            _knownKeys.Add("1156943126", "Ultimate Galactic Conquest Custom Edition");
            _knownKeys.Add("1197705959", "Star Wars Return of the Clones");
            _knownKeys.Add("1382582782", "Absolute Chaos");
            _knownKeys.Add("1301047166", "Nojembre's Republic at War Submod");
            _knownKeys.Add("1126880602", "Stargate - Empire at War: Pegasus Chronicles");
            _knownKeys.Add("1241979729", "FOC Alliance Rebellion");
        }

        public static bool TryFind(string key, out string name)
        {
            name = default;
            if (_knownKeys.ContainsKey(key))
            {
                name = _knownKeys[key];
                return true;
            }
            return false;
        }
    }
}