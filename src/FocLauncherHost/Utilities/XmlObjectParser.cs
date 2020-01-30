using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FocLauncherHost.Utilities
{
    public class XmlObjectParser<T> where T : class
    {
        private Stream FileStream { get; }

        public XmlObjectParser(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public XmlObjectParser(Stream dataStream)
        {
            if (dataStream == null || dataStream.Length == 0)
                throw new ArgumentNullException(nameof(dataStream));
            if (!dataStream.CanRead)
                throw new NotSupportedException();
            FileStream = dataStream;
        }

        public T Parse()
        {
            FileStream.Seek(0, SeekOrigin.Begin);
            var reader = XmlReader.Create(FileStream,
                new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Document });
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }
    }
}
