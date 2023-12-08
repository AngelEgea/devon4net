using Devon4Net.Infrastructure.Common.Helpers.Interfaces;
using Devon4Net.Infrastructure.Common.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Devon4Net.Infrastructure.Common.Helpers
{
    public class XmlHelper : IDisposable, IXmlHelper
    {
        private XmlSchemaSet XmlSchemaSet { get; set; }
        private XmlReader XmlReader { get; set; }
        private XDocument XDocument { get; set; }
        private bool ThrowError { get; }
        private string ValidationMessage { get; set; }

        public XmlHelper(bool throwError = false)
        {
            XmlSchemaSet = new XmlSchemaSet();
            ThrowError = throwError;
            Reset();
        }

        public void Reset()
        {
            XmlSchemaSet = new XmlSchemaSet();
            ValidationMessage = string.Empty;
            XDocument = null;
        }

        public void AddXsdFolder(string xsdFolderPath)
        {
            if (Directory.Exists(xsdFolderPath))
            {
                foreach (var file in Directory.GetFiles(xsdFolderPath))
                {
                    XmlSchemaSet.Add(null, CheckFile(file));
                }
            }
        }

        public void AddXsdFile(string xsdPath)
        {
            XmlSchemaSet.Add(null, CheckFile(xsdPath));
        }

        public void LoadXmlFile(Stream xmlPath)
        {
            XmlReader = XmlReader.Create(xmlPath);
            XDocument = XDocument.Load(XmlReader);
        }

        public bool ValidateXml()
        {
            XDocument.Validate(XmlSchemaSet, ValidationEventHandler);
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public string GetErrorMessage()
        {
            return ValidationMessage;
        }

        private static string CheckFile(string filePath)
        {
            var theFile = FileOperations.GetFileFullPath(filePath);
            if (string.IsNullOrEmpty(theFile))
            {
                throw new ArgumentException($"The file {filePath} was not found");
            }

            return theFile;
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (Enum.TryParse("Error", out XmlSeverityType type) && type == XmlSeverityType.Error)
            {
                ValidationMessage = e.Message;
                if (ThrowError)
                {
                    throw new ArgumentException(ValidationMessage);
                }
            }
            else
            {
                ValidationMessage = string.Empty;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            XmlReader?.Dispose();
            XmlSchemaSet = null;
            XDocument = null;
        }
    }
}
