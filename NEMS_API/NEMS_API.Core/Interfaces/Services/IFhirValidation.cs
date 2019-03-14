using Hl7.Fhir.Model;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IFhirValidation
    {
        OperationOutcome ValidProfile<T>(T resource, string customProfile) where T : Resource;

        OperationOutcome ValidSubscription(Subscription subscription);
    }
}
