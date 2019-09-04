using Hl7.Fhir.Model;
using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;

namespace NEMS_API.Models.FhirResources
{
    public class NemsMessageHeader
    {
        private readonly NemsApiSettings _nemsApiSettings;

        public NemsMessageHeader(ResourceUrls urls)
        {
            _nemsApiSettings = new NemsApiSettings { ResourceUrl = urls };
        }

        public Guid Id { get; set; }

        public KeyValuePair<string, string> EvetType { get; set; }

        public Guid MainFocusId { get; set; } //communicationId

        public MessageHeader Default()
        {
            var messageHeader = new MessageHeader
            {
                Id = Id.ToString(),
                Meta = new Meta
                {
                    Profile = new string[] { _nemsApiSettings.ResourceUrl.HeaderProfileUrl }
                },
                Timestamp = DateTimeOffset.Now,
                Event = new Coding
                {
                    System = _nemsApiSettings.ResourceUrl.EventCodesUrl,
                    Display = EvetType.Value,
                    Code = EvetType.Key
                },
                Responsible = new ResourceReference
                {
                    Reference = "https://directory.spineservices.nhs.uk/STU3/Organization/X26",
                    Display = "NHS Digital"
                },
                Source = new MessageHeader.MessageSourceComponent
                {
                    Endpoint = "urn:nhs:addressing:asid:477121000323"
                },
                Extension = new List<Extension>
                {
                    new Extension
                    {
                        Url = "https://fhir.nhs.uk/STU3/StructureDefinition/Extension-EMS-MessageEventType-1",
                        Value = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "https://fhir.nhs.uk/STU3/CodeSystem/EMS-MessageEventType-1",
                                    Display = "New event message",
                                    Code = "new"
                                }
                            }
                        }
                    }
                },
                Focus = new List<ResourceReference>
                {
                    new ResourceReference
                    {
                        Reference = $"urn:uuid:{MainFocusId}"
                    }
                }
            };

            return messageHeader;
        }
    }
}
