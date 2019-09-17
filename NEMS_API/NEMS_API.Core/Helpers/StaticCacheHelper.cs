using Microsoft.Extensions.Caching.Memory;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace NEMS_API.Core.Helpers
{
    public class StaticCacheHelper : IStaticCacheHelper
    {
        private IMemoryCache _cache;
        private DateTimeOffset _globalExpiration => GetGlobalExpiration();

        public StaticCacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T GetEntry<T>(string key) where T : class, new()
        {
            var dataOut = GetCacheData<T>(key);

            return dataOut;
        }

        //public List<T> GetListEntry<T>(string key) where T : class, new()
        //{
        //    var dataOut = GetCacheData<List<T>>(key);

        //    return dataOut;
        //}

        public T AddEntry<T>(T entry) where T : IDataItem
        {
            AddEntry(entry, entry.CacheKey);

            return entry;
        }

        public T AddEntry<T>(T entry, string cacheKey)
        {
            Set(cacheKey, entry, _globalExpiration);

            return entry;
        }

        public T AddListItem<T>(T entry) where T: IDataItem, new()
        {
            var dataOut = new List<T>();

            if(!_cache.TryGetValue(entry.CacheKey, out dataOut))
            {
                dataOut = new List<T>();
                // Save data in cache.
                //Set(key, dataOut, _globalExpiration);
            }

            dataOut.Add(entry);

            //var oType = dataOut.GetType();
            //if(oType.GetTypeInfo().IsGenericType && oType.GetGenericTypeDefinition() == typeof(List<>))
            //{
            //(dataOut as List<Tc>).Add(entry);
            //}
            //else
            //{
            //Tc and Te should be same here
            //dataOut = entry as Tc;
            //}

            Set(entry.CacheKey, dataOut, _globalExpiration);

            return entry;
        }

        public void RemoveListItem<T>(T entry) where T : IDataItem
        {
            var dataOut = new List<T>();

            if (_cache.TryGetValue(entry.CacheKey, out dataOut))
            {
                dataOut.RemoveAll((x) => { return x.Id == entry.Id; });

                Set(entry.CacheKey, dataOut, _globalExpiration);
            }
        }

        public void RemoveEntry(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            using (var entry = _cache.CreateEntry(key))
            {
                entry.Value = value;
                entry.AbsoluteExpiration = absoluteExpiration;
                //TODO: entry.Size = 1;
            }
        }

        private DateTimeOffset GetGlobalExpiration()
        {
            var now = DateTime.UtcNow;

            int hour = (now.Hour >= 20) ? 8 : 20;
            return new DateTimeOffset(new DateTime(now.Year, now.Month, now.Day, hour, 0, 0, 0, DateTimeKind.Utc));
        }

        private T GetCacheData<T>(string key) where T : class, new()
        {
            var dataOut = new T();

            _cache.TryGetValue<T>(key, out dataOut);

            return dataOut;
        }

        //private Type ListType(Type type)
        //{
        //    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        //    {
        //        return type.GetGenericArguments()[0];
        //    }

        //    return null;
        //}
    }
}
