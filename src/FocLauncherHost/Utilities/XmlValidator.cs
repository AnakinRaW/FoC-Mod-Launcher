using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace FocLauncherHost.Utilities
{
    public class XmlValidator
    {
        private int _errors;

        public XmlValidator(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(nameof(file));

            SchemeFileStream = new FileStream(file, FileMode.Open);
        }

        public XmlValidator(Stream schemeFileStream)
        {
            SchemeFileStream = schemeFileStream;
        }

        private Stream SchemeFileStream { get; }

        public bool Validate(string filePath, ConformanceLevel conformanceLevel = ConformanceLevel.Auto)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return InternalValidate(stream, conformanceLevel);
        }

        public bool Validate(Stream stream, ConformanceLevel conformanceLevel = ConformanceLevel.Auto)
        {
            return InternalValidate(stream, conformanceLevel);
        }

        public bool Validate(XmlNode node, ConformanceLevel conformanceLevel = ConformanceLevel.Auto)
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.ImportNode(node, true));
            using var stream = new MemoryStream();
            doc.Save(stream);
            return InternalValidate(stream, conformanceLevel);
        }

        private bool InternalValidate(Stream fileStream, ConformanceLevel conformanceLevel)
        {
            fileStream.Position = 0;
            SchemeFileStream.Position = 0;
            bool result;
            try
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation |
                                            XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ConformanceLevel = conformanceLevel;
                settings.ValidationEventHandler += Settings_ValidationEventHandler;
                if (SchemeFileStream != null)
                {
                    using var schemaReader = XmlReader.Create(SchemeFileStream);
                    settings.Schemas.Add(null, schemaReader);
                }

                var reader = XmlReader.Create(fileStream, settings);
                while (reader.Read())
                {
                }
                reader.Close();
                if (_errors > 0)
                    throw new Exception();
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            finally
            {
                SchemeFileStream?.Dispose();
            }
            return result;
        }

        private void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            _errors++;
        }
    }
}
