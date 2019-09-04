namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteriaRule
    {
        public string Parameter { get; set; }

        public string ValueType { get; set; }

        public string Values { get; set; }

        public bool IsPrefix { get; set; }

        public bool Ignore { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }
    }
}
