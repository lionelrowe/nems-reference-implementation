namespace NEMS_API.Models.Core
{
    public class CacheKeys
    {
        public static string PatientEntries = "Patients";

        public static string SubscriptionEntries = "SubscriptionEntries";

        public static string SdsEntries = "SdsEntries";

        public static string SchemaProfile(string type) => "SchemaProfiles:" + type;

    }
}
