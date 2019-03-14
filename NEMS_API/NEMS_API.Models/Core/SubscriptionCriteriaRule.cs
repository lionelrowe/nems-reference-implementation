namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteriaRule
    {
        public SubscriptionCriteriaRule()
        {
            Key = "NOT_SET";
        }

        public string Key { get; set; }

        public string Value { get; set; }

        public bool ExactValue { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }
    }
}
