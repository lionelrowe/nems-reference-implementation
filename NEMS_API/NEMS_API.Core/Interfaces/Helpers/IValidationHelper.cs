using Hl7.Fhir.Model;
using Hl7.Fhir.Validation;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IValidationHelper
    {
        Validator Validator { get; }
    }
}
