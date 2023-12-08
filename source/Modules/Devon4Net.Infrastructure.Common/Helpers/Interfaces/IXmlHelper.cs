namespace Devon4Net.Infrastructure.Common.Helpers.Interfaces
{
    public interface IXmlHelper
    {
        void AddXsdFile(string xsdPath);
        void AddXsdFolder(string xsdFolderPath);
        string GetErrorMessage();
        void LoadXmlFile(Stream xmlPath);
        void Reset();
        bool ValidateXml();
    }
}