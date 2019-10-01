using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Core.Helpers
{
    public static class FhirHelper
    {

        public static T GetFirstEntryOfTypeProfile<T>(Bundle bundle, string profileUrl) where T : Resource
        {
            var typeOf = typeof(T);

            if (bundle?.Entry == null || bundle.Entry.Count == 0 || string.IsNullOrEmpty(profileUrl))
            {
                return null;
            }

            var resource = bundle.Entry.ElementAt(0).Resource;

            if (resource == null || resource.ResourceType.ToString() != typeOf.Name || resource.Meta == null || resource.Meta.Profile.All(y => y != profileUrl))
            {
                return null;
            }

            return resource as T;
        }

        public static Bundle GetAsSearchset<T>(List<T> entries, string entriesPath, string selfLink) where T : Resource
        {
            var bundle = new Bundle();
            bundle.Type = Bundle.BundleType.Searchset;
            bundle.Id = Guid.NewGuid().ToString();
            bundle.Link = new List<Bundle.LinkComponent>
            {
                new Bundle.LinkComponent
                {
                    Relation = "self",
                    Url = selfLink
                }
            };
            bundle.Total = entries.Count();
            bundle.Entry = new List<Bundle.EntryComponent>();
            bundle.Entry.AddRange(entries.Select(x => new Bundle.EntryComponent { FullUrl = $"{entriesPath}/{x.Id}", Resource = x, Search = new Bundle.SearchComponent { Mode = Bundle.SearchEntryMode.Match } }).ToList());

            return bundle;
        }
    }
}
