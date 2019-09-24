using Hl7.Fhir.Model;
using NEMS_API.Models.Core;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface ISubscribeService
    {
        Subscription ReadEvent(FhirRequest request);

        Resource CreateEvent(FhirRequest request);

        void DeleteEvent(FhirRequest request);
    }
}
