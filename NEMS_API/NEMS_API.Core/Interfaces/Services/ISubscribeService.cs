using Hl7.Fhir.Model;
using NEMS_API.Models.Core;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface ISubscribeService
    {
        Subscription ReadEvent(FhirRequest request);

        List<Subscription> SearchEvent(FhirRequest request);

        Resource CreateEvent(FhirRequest request);

        void DeleteEvent(FhirRequest request);
    }
}
