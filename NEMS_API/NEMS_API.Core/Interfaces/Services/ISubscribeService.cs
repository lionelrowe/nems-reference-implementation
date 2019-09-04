using Hl7.Fhir.Model;
using NEMS_API.Models.FhirResources;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface ISubscribeService
    {
        Subscription ReadEvent(string id);

        Resource CreateEvent(Subscription subscription);

        void DeleteEvent(string id);
    }
}
