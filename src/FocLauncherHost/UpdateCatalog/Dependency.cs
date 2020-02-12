using System;
using System.Xml.Serialization;

namespace FocLauncherHost.UpdateCatalog
{
    [Serializable]
    public class Dependency
    {
        private string _destination;
        private bool _requiresRestart;
        private string _origin;
        private byte[] _sha2;
        private string _name;
        private string _version;

        public string Destination
        {
            get => _destination;
            set => _destination = value;
        }

        public bool RequiresRestart
        {
            get => _requiresRestart;
            set => _requiresRestart = value;
        }

        [XmlElement(DataType = "anyURI")]
        public string Origin
        {
            get => _origin;
            set => _origin = value;
        }

        [XmlElement(DataType = "hexBinary")]
        public byte[]? Sha2
        {
            get => _sha2;
            set => _sha2 = value;
        }

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [XmlAttribute]
        public string Version
        {
            get => _version;
            set => _version = value;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}, {nameof(Destination)}: {Destination}, {nameof(Origin)}: {Origin}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Dependency)obj);
        }

        internal Version? GetVersion()
        {
            if (string.IsNullOrEmpty(Version))
                return null;
            return !System.Version.TryParse(Version, out var version) ? null : version;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        protected bool Equals(Dependency other)
        {
            return _name == other._name;
        }

    }
}