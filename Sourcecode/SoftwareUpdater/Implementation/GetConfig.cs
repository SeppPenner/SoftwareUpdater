using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using SoftwareUpdater.Configuration;
using SoftwareUpdater.Interface;

namespace SoftwareUpdater.Implementation
{
    public class GetConfig : IGetConfig
    {
        public Config ImportConfiguration(string fileName)
        {
            var xDocument = XDocument.Load(fileName);
            return CreateObjectFromString<Config>(xDocument);
        }

        private T CreateObjectFromString<T>(XDocument xDocument)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T) xmlSerializer.Deserialize(new StringReader(xDocument.ToString()));
        }
    }
}