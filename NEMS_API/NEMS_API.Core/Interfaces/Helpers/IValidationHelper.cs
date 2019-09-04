using Hl7.Fhir.Model;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IValidationHelper
    {
        OperationOutcome ValidateResource<T>(T resource, string resourceSchema) where T : Resource;

        CodeSystem GetCodeSystem(string system);

        SearchParameter GetSearchParameter(string system);
    }
}
