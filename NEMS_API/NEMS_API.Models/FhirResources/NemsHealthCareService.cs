using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace NEMS_API.Models.FhirResources
{
    public class NemsHealthCareService
    {
        public Guid Id { get; set; }

        public HealthcareService Default()
        {
            var healthCareService = new HealthcareService
            {
                Id = Id.ToString(),
                Meta = new Meta
                {
                    Profile = new string[] { "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-HealthcareService-1" },
                },
                ProvidedBy = new ResourceReference
                {
                    Reference = "https://directory.spineservices.nhs.uk/STU3/Organization/X26",
                    Display = "NHS Digital"
                },
                Type = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "https://fhir.nhs.uk/STU3/CodeSystem/EMS-HealthcareServiceType-1",
                                Display = "Personal Demographics Service",
                                Code = "PDS"
                            }
                        }
                    }
                }
            };

            return healthCareService;
        }
    }
}
