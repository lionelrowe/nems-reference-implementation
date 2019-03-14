using Hl7.Fhir.Model;
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
    }
}
