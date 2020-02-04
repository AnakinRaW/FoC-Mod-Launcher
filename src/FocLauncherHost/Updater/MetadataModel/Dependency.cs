using System;
using System.Xml.Serialization;

namespace FocLauncherHost.Updater.MetadataModel
{
    [Serializable]
    public class Dependency
    {
        private InstallLocation _installLocationField;
        private bool _requiresRestartField;
        private string _sourceLocationField;
        private byte[] _sha2Field;
        private string _nameField;
        private string _versionField;

        public InstallLocation InstallLocation
        {
            get => _installLocationField;
            set => _installLocationField = value;
        }

        public bool RequiresRestart
        {
            get => _requiresRestartField;
            set => _requiresRestartField = value;
        }

        [XmlElement(DataType = "anyURI")]
        public string SourceLocation
        {
            get => _sourceLocationField;
            set => _sourceLocationField = value;
        }

        [XmlElement(DataType = "hexBinary")]
        public byte[]? Sha2
        {
            get => _sha2Field;
            set => _sha2Field = value;
        }

        [XmlAttribute]
        public string Name
        {
            get => _nameField;
            set => _nameField = value;
        }

        [XmlAttribute]
        public string Version
        {
            get => _versionField;
            set => _versionField = value;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Version)}: {Version}, {nameof(InstallLocation)}: {InstallLocation}, {nameof(SourceLocation)}: {SourceLocation}";
        }

    }
}