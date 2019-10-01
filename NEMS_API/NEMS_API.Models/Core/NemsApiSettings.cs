using System.Collections.Generic;

namespace NEMS_API.Models.Core
{
    public class NemsApiSettings
    {
        public NemsApiSettings()
        {
            SupportedContentTypes = new List<KvList>();
            SubscriptionCriteriaRules = new List<SubscriptionCriteriaRule>();
            BundleValidationSchemas = new Dictionary<string, List<KvList>>();
            SupportedEventTypes = new List<string>();
        }

        public ResourceUrls ResourceUrl { get; set; }

        public List<KvList> SupportedContentTypes { get; set; }

        public List<string> SupportedEventTypes { get; set; }

        public List<SubscriptionCriteriaRule> SubscriptionCriteriaRules { get; set; }


        public bool SkipSubscriptionCriteria { get; set; }

        public bool MessageHeaderValidationOnly { get; set; }

        public bool SkipSpineGateWay { get; set; }

        public string PatientFile { get; set; }

        public string SdsFile { get; set; }

        public string SpineASID { get; set; }

        public string MeshMailboxId { get; set; }

        public List<InteractionIdMap> InteractionIdMap { get; set; }

        public IDictionary<string, List<KvList>> BundleValidationSchemas { get; set; }

    }

    public class ResourceUrls
    {
        public string BundleProfileUrl { get; set; }

        public string SubscriptionProfileUrl { get; set; }

        public string HeaderProfileUrl { get; set; }

        public string EventCodesUrl { get; set; }

        public string NhsNumberSystem { get; set; }

        public string OrganisationReferenceUrl { get; set; }
    }

    public class KvList
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
