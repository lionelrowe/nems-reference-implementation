using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using NEMS_API.Core.Interfaces.Helpers;

namespace NEMS_API.Core.Helpers
{
    public class ValidationHelper : IValidationHelper
    {
        public Validator Validator { get; }

        private IResourceResolver _source { get; }

        private readonly IFhirCacheHelper _fhirCacheHelper;

        public ValidationHelper(IFhirCacheHelper fhirCacheHelper)
        {
            _fhirCacheHelper = fhirCacheHelper;

            _source = _fhirCacheHelper.GetSource();

            var ctx = new ValidationSettings()
            {
                ResourceResolver = _source,
                GenerateSnapshot = true,
                EnableXsdValidation = false,
                Trace = false,
                ResolveExteralReferences = true
            };


            Validator = new Validator(ctx);
        }
    }
}
