using System.Collections.Generic;

namespace NEMS_API.Models.Core
{
    public class NemsApiSettings
    {
        public NemsApiSettings()
        {
            SupportedContentTypes = new List<string>();
            SubscriptionCriteriaRules = new List<string>();
            ServiceTypeCodes = new List<string>();
        }

        public string BundleProfileUrl { get; set; }

        public string SubscriptionProfileUrl { get; set; }

        public string HeaderProfileUrl { get; set; }

        public List<string> SupportedContentTypes { get; set; }

        public List<string> SubscriptionCriteriaRules { get; set; }

        public List<string> ServiceTypeCodes { get; set; }
    }
}
