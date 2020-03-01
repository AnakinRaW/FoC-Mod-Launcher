using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FocLauncherHost.Utilities;
using TaskBasedUpdater;

namespace FocLauncherHost.Update.UpdateCatalog
{
    [Serializable]
    [XmlRoot("Products", Namespace = "", IsNullable = false)]
    public class Catalogs
    {
        private List<ProductCatalog> _products = new List<ProductCatalog>();

        [XmlElement("Product")]
        public List<ProductCatalog> Products
        {
            get => _products;
            set => _products = value;
        }

        public static async Task<Catalogs> DeserializeAsync(Stream stream)
        {
            if (stream == null || stream.Length == 0)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new NotSupportedException();

            var parser = new XmlObjectParser<Catalogs>(stream);
            return await Task.FromResult(parser.Parse());
        }

        internal ProductCatalog? GetMatchingProduct(IProductInfo product)
        {
            if (_products == null || !_products.Any())
                throw new NotSupportedException("No products to update are found");

            return _products.FirstOrDefault(x => x.Name.Equals(product.Name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}