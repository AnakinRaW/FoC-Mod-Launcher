using System;
using System.Xml.Serialization;

namespace FocLauncherHost.Updater.MetadataModel
{
    [Serializable]
    public class Dependency : IDependency
    {
        private InstallLocation _installLocation;
        private bool _requiresRestart;
        private string _sourceLocation;
        private byte[] _sha2;
        private string _name;
        private string _version;

        public InstallLocation InstallLocation
        {
            get => _installLocation;
            set => _installLocation = value;
        }

        public bool RequiresRestart
        {
            get => _requiresRestart;
            set => _requiresRestart = value;
        }

        [XmlElement(DataType = "anyURI")]
        public string SourceLocation
        {
            get => _sourceLocation;
            set => _sourceLocation = value;
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

        [XmlIgnore]
        public DependencyAction RequiredAction { get; set; }

        [XmlIgnore]
        public CurrentDependencyState CurrentState { get; set; }

        public Version? GetVersion()
        {
            try
            {
                return System.Version.TryParse(_version, out var version) ? version : null;
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}, {nameof(InstallLocation)}: {InstallLocation}, {nameof(SourceLocation)}: {SourceLocation}";
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Dependency)obj);
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