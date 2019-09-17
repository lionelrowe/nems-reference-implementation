using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NEMS_API.Core.Interfaces.Helpers;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;

namespace NEMS_API.Core.Helpers
{
    public class FileHelper : IFileHelper
    {
        public string GetStringFromFile(string relativeFilePath)
        {
            var basePath = DirectoryHelper.GetBaseDirectory();

            var data = File.ReadAllText(Path.Combine(basePath, relativeFilePath));

            return data;
        }

        public T GetResourceFromFile<T>(string relativeFilePath, bool isXml) where T : class
        {
            var data = GetStringFromFile(relativeFilePath);

            var isFhir = typeof(T).IsSubclassOf(typeof(Resource)) || typeof(T) == typeof(Resource);

            var dataOut = ParseResource<T>(data, isFhir, isXml);

            return dataOut;
        }

        private T ParseResource<T>(string content, bool isFhir, bool isXml) where T : class
        {
            if(isFhir)
            {
                if(isXml)
                {
                    var parser = new FhirXmlParser();
                    return parser.Parse(content, typeof(T)) as T;
                }
                else
                {
                    var parser = new FhirJsonParser();
                    return parser.Parse(content, typeof(T)) as T;
                }
            }
            else
            {
                if(isXml)
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using (TextReader reader = new StringReader(content))
                    {
                        return (T) serializer.Deserialize(reader);
                    }
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(content) as T;
                }
            }
        }
    }
}
