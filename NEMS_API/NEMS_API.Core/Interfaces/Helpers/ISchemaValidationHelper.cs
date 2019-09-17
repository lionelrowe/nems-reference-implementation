using Hl7.Fhir.Model;
using NEMS_API.Models.Core;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface ISchemaValidationHelper
    {
        IList<string> ValidateFhirResource<T>(T resource, SchemaProfile schemaProfile) where T : Resource;
    }
}
