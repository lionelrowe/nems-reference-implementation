using NEMS_API.Models.Interfaces;

namespace NEMS_API.Models.Core
{
    public abstract class DataItem : IDataItem
    {
        public string Id { get; set; }

        public string Data { get; set; }

        public abstract string CacheKey { get; }
    }
}
