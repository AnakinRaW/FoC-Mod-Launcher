using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FocLauncherHost.Updater.MetadataModel
{
    [Serializable]
    public class ProductMetadata
    {
        private List<Dependency> _dependencies = new List<Dependency>();
        private string _name;
        private string _author;

        [XmlArrayItem("Dependency", IsNullable = false)]
        public List<Dependency> Dependencies
        {
            get => _dependencies;
            set => _dependencies = value;
        }

        [XmlAttribute]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [XmlAttribute]
        public string Author
        {
            get => _author;
            set => _author = value;
        }
    }
}
