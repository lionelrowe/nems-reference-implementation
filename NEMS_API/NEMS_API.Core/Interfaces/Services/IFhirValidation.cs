using Hl7.Fhir.Model;
using NEMS_API.Models.FhirResources;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IFhirValidation
    {
        OperationOutcome ValidProfile<T>(T resource, string customProfile) where T : Resource;

        OperationOutcome ValidSubscription(NemsSubscription subscription);
    }
}
