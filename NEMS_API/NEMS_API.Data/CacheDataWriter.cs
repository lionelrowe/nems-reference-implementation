using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Models.Interfaces;
using System;

namespace NEMS_API.Data
{
    public class CacheDataWriter : IDataWriter
    {
        private readonly IStaticCacheHelper _staticCacheHelper;

        public CacheDataWriter(IStaticCacheHelper staticCacheHelper)
        {
            _staticCacheHelper = staticCacheHelper;
        }

        public T Create<T>(T data) where T : IDataItem, new()
        {
            var entry = _staticCacheHelper.AddListItem(data);
            return data;
        }

        public T Create<T>(T data, DateTimeOffset lifespan) where T : IDataItem, new()
        {
            var entry = _staticCacheHelper.AddListItem(data);
            return data;
        }

        public T Create<T>(T data, string entryId, DateTimeOffset lifespan)
        {
            var entry = _staticCacheHelper.AddEntry(data, entryId, lifespan);
            return data;
        }

        public void Delete<T>(T entry) where T : IDataItem
        {
            _staticCacheHelper.RemoveListItem(entry);
        }
    }
}
