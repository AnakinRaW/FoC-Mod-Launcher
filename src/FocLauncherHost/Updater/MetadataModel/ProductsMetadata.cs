using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FocLauncherHost.Utilities;

namespace FocLauncherHost.Updater.MetadataModel
{
    [Serializable]
    [XmlRoot("Products", Namespace = "", IsNullable = false)]
    public class ProductsMetadata
    {
        private List<ProductMetadata> _products = new List<ProductMetadata>();

        [XmlElement("Product")]
        public List<ProductMetadata> Products
        {
            get => _products;
            set => _products = value;
        }

        public static async Task<ProductsMetadata> DeserializeAsync(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new NotSupportedException();

            var parser = new XmlObjectParser<ProductsMetadata>(stream);
            return await Task.FromResult(parser.Parse());
        }
    }
}