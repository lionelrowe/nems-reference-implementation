using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Models.FhirResources
{
    public class NemsCommunication
    {
        public Guid Id { get; set; }

        public Patient Patient { get; set; }

        public HumanName PatientName => Patient?.Name?.FirstOrDefault(x => x.Use == HumanName.NameUse.Official);

        public Communication Default()
        {
            var communication = new Communication
            {
                Id = Id.ToString(),
                Meta = new Meta
                {
                    Profile = new string[] { "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-MessageHeader-1" }
                },
                Sent = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Status = EventStatus.Completed,
                Subject = new ResourceReference
                {
                    Reference = $"urn:uuid:{Patient.Id}",
                    Display = $"{PatientName.Family.ToUpperInvariant()}, {string.Join(" ", PatientName.Given)}"
                },
                Payload = new List<Communication.PayloadComponent>
                            {
                                new Communication.PayloadComponent
                                {
                                    Content = new ResourceReference
                                    {
                                        Reference = $"urn:uuid:{Patient.Id}",
                                        Display = $"{PatientName.Family.ToUpperInvariant()}, {string.Join(" ", PatientName.Given)}"
                                    }
                                }
                            }
            };

            return communication;
        }
    }
}
