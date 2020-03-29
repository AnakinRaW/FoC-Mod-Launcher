using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FocLauncher.Utilities;
using NLog;

namespace FocLauncher.UpdateMetadata
{
    [Serializable]
    [XmlRoot("Products", Namespace = "", IsNullable = false)]
    public class Catalogs
    {
        private List<ProductCatalog> _products = new List<ProductCatalog>();

        [XmlIgnore]
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        public static async Task<Catalogs?> TryDeserializeAsync(Stream stream)
        {
            try
            {
                Logger.Trace("Try deserializing stream to Catalogs");
                return  await DeserializeAsync(stream);
            }
            catch (Exception e)
            {
                Logger.Debug(e, "Getting catalogs from stream failed with exception. Returning null instead.");
                return null;
            }
        }
    }
}