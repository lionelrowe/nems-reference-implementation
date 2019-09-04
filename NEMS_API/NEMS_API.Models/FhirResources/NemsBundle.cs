using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace NEMS_API.Models.FhirResources
{
    public class NemsBundle
    {
        public NemsBundle()
        {
            Entries = new List<Bundle.EntryComponent>();
        }

        public Guid Id { get; set; }

        public List<Bundle.EntryComponent> Entries { get; set; }

        public void AddEntry(string id, Resource resource)
        {
            Entries.Add(new Bundle.EntryComponent
            {
                FullUrl = $"urn:uuid:{id}",
                Resource = resource
            });
        }

        public void AddEntry(Guid id, Resource resource)
        {
            AddEntry(id.ToString(), resource);
        }

        public Bundle Default()
        {
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Message,
                Id = Id.ToString(),
                Meta = new Meta
                {
                    Profile = new string[] { "https://fhir.nhs.uk/STU3/StructureDefinition/EMS-Bundle-1" }
                },
                Entry = Entries
            };

            return bundle;
        }
    }
}
