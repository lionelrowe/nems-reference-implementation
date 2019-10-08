using Hl7.Fhir.Model;
using NEMS_API.Models.Core;
using NEMS_API.Models.MessageExchange;
using System.Collections.Generic;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IPublishService
    {
        SystemTasks.Task<Resource> ValidateEvent(FhirRequest request);

        SubscriptionMatchingCriteria GetSubscriptionMatchingCriteria(Bundle bundle);

        void PublishEvent(Bundle bundle, List<string> mailboxIds, string workflowId);
    }
}
