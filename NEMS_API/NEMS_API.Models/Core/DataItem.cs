using NEMS_API.Models.Interfaces;

namespace NEMS_API.Models.Core
{
    public class DataItem : IDataItem
    {
        public string Id { get; set; }

        public string CacheKey { get; set; }
    }
}
