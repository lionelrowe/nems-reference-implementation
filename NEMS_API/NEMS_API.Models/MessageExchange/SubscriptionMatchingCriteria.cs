using Hl7.Fhir.Model;
using NEMS_API.Models.Core;

namespace NEMS_API.Models.MessageExchange
{
    public class SubscriptionMatchingCriteria
    {
        public string PatientIdentifier { get; set; }

        public FhirDateTime BirthDate { get; set; }

        public EventTypeConfig ActiveEventType { get; set; }
    }
}
