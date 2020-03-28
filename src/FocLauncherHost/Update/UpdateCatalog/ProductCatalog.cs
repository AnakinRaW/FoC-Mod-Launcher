using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FocLauncherHost.Update.UpdateCatalog
{
    [Serializable]
    public class ProductCatalog
    {
        private List<Dependency> _dependencies = new List<Dependency>();

        private string _name;
        private string _author;
        private PreviewType _preview;

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

        [XmlAttribute]
        public PreviewType Preview
        {
            get => _preview;
            set => _preview = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Launcher: ");
            sb.Append($"Name: {Name}, ");
            sb.Append($"Preview: {Preview}");
            sb.Append($"Author: {Author}");

            if (!Dependencies.Any())
                return sb.ToString();

            var dependencySb = new StringBuilder();
            foreach (var dependency in Dependencies) 
                dependencySb.AppendLine("\t" + dependency);

            sb.AppendLine($"Dependencies ({Dependencies.Count}):");
            sb.Append(dependencySb);
            return sb.ToString();
        }
    }
}
