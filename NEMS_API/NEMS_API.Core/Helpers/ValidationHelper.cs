using Hl7.Fhir.Model;
using NEMS_API.Core.Interfaces.Helpers;

namespace NEMS_API.Core.Helpers
{
    public class ValidationHelper : IValidationHelper
    {
        private readonly IFhirResourceHelper _fhirResourceHelper;

        public ValidationHelper(IFhirResourceHelper fhirResourceHelper)
        {
            _fhirResourceHelper = fhirResourceHelper;
        }

        public OperationOutcome ValidateResource<T>(T resource, string resourceSchema) where T : Resource
        {
            return _fhirResourceHelper.ValidateResource(resource, resourceSchema);
        }

        public CodeSystem GetCodeSystem(string system)
        {
            return _fhirResourceHelper.GetCodeSystem(system);
        }

        public SearchParameter GetSearchParameter(string system)
        {
            return _fhirResourceHelper.GetSearchParameter(system) as SearchParameter;
        }
    }
}
