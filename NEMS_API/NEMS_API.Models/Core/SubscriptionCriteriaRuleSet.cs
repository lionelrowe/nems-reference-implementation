using System.Collections.Generic;

namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteriaRuleSet
    {
        public SubscriptionCriteriaRuleSet()
        {
            Rules = new List<SubscriptionCriteriaRule>();
        }

        public string StartsWith { get; set; }

        public List<SubscriptionCriteriaRule> Rules { get; set; }
    }
}
