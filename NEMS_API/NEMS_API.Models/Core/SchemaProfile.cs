namespace NEMS_API.Models.Core
{
    public class SchemaProfile : DataItem
    {
        public string CacheKeyType { get; set; }

        public string SchemaPathBase { get; set; }

        public override string CacheKey
        {
            get
            {
                return CacheKeys.SchemaProfile(CacheKeyType);
            }
        }
    }
}
