using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Models.Core;
using NEMS_API.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace NEMS_API.Core.Helpers
{
    public class SchemaValidationHelper : ISchemaValidationHelper
    {
        public IList<string> ValidateFhirResource<T>(T resource, SchemaProfile schemaProfile) where T : Resource
        {
            var schema = JSchema.Parse(schemaProfile.Data);

            var resourceJson = new FhirJsonSerializer(new SerializerSettings { Pretty = true }).SerializeToString(resource);
            var res = JObject.Parse(resourceJson);

            IList<ValidationError> messages;
            bool valid = res.IsValid(schema, out messages);

            return valid ? null : BuildErrors(messages, schemaProfile.SchemaPathBase);

        }

        private IList<string> BuildErrors(IList<ValidationError> errors, string basePath)
        {
            var validationMessages = new List<string>();

            foreach (var err in errors)
            {
                validationMessages.Add($"Value at {basePath}.{err.Path} is invalid. {err.Message}");

                validationMessages.AddRange(BuildErrors(err.ChildErrors, basePath));
            }

            return validationMessages;
        }
    }
}
