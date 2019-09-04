using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace NEMS_API.Models.FhirResources
{
    public class NemsDeathNotification
    {

        public Extension Default()
        {
            var extension = new Extension
            {
                Url = "https://fhir.hl7.org.uk/STU3/StructureDefinition/Extension-CareConnect-DeathNotificationStatus-1",
                Extension = new List<Extension>
                {
                    new Extension
                    {
                        Url = "systemEffectiveDate",
                        Value = FhirDateTime.Now()
                    },
                    new Extension
                    {
                        Url = "deathNotificationStatus",
                        Value = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = "U",
                                    System = "https://fhir.hl7.org.uk/STU3/CodeSystem/CareConnect-DeathNotificationStatus-1"
                                }
                            }
                        }
                    }
                }
            };

            return extension;
        }
    }
}
