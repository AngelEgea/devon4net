using System.Xml;
using System.Xml.Serialization;

namespace Devon4Net.Infrastructure.Common.IO
{
    public static class XmlOperations
    {
        public static T DeserializeXmlToObject<T>(string content) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using TextReader reader = new StringReader(content);
            return (T)ser.Deserialize(reader);
        }

        public static string SerializeObjectToXml<T>(T obj) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using var sww = new StringWriter();
            using XmlTextWriter writer = new XmlTextWriter(sww) { Formatting = Formatting.Indented };
            ser.Serialize(writer, obj);
            return sww.ToString();
        }

        public static T DeserializeXmlToObjectFromXmlStream<T>(Stream stream) where T : class
        {
            try
            {
                using var reader = new StreamReader(stream);
                string content = reader.ReadToEnd();

                return DeserializeXmlToObject<T>(content);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}