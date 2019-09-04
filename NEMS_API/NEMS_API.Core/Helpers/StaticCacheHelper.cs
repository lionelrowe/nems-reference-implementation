using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Caching.Memory;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NEMS_API.Core.Helpers
{
    public class StaticCacheHelper : IStaticCacheHelper
    {
        private IMemoryCache _cache;
        private IFileHelper _fileHelper;
        private DateTimeOffset _globalExpiration => GetGlobalExpiration();

        public StaticCacheHelper(IMemoryCache cache, IFileHelper fileHelper)
        {
            _cache = cache;
            _fileHelper = fileHelper;
        }

        public T GetDataItem<T>(string key, string location) where T : class, new()
        {
            var dataOut = GetAndSetCacheData<T>(key, location);

            return dataOut;
        }

        public List<T> GetDataList<T>(string key, string location) where T : class, new()
        {
            var dataOut = GetAndSetCacheData<List<T>>(key, location);

            return dataOut;
        }

        //public List<T> GetCacheData<T>(string key) where T : new()
        //{
        //    var dataOut = new List<T>();

        //    var found =_cache.TryGetValue<List<T>>(key, out dataOut);

        //    return found ? dataOut : new List<T>();
        //}

        public T AddItem<T>(T entry) where T: IDataItem, new()
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

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
        {
            using (var entry = _cache.CreateEntry(key))
            {
                entry.Value = value;
                entry.AbsoluteExpiration = absoluteExpiration;
                //TODO: entry.Size = 1;
            }
        }

        public void RemoveItem<T>(T entry) where T : IDataItem
        {
            var dataOut = new List<T>();

            if (_cache.TryGetValue(entry.CacheKey, out dataOut))
            {
                dataOut.RemoveAll((x) => { return x.Id == entry.Id; });

                Set(entry.CacheKey, dataOut, _globalExpiration);
            }
        }

        public void RemoveAll(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        private DateTimeOffset GetGlobalExpiration()
        {
            var now = DateTime.UtcNow;

            int hour = (now.Hour >= 20) ? 8 : 20;
            return new DateTimeOffset(new DateTime(now.Year, now.Month, now.Day, hour, 0, 0, 0, DateTimeKind.Utc));
        }

        private T GetAndSetCacheData<T>(string key, string location) where T : class, new()
        {
            var dataOut = new T();

            if (!_cache.TryGetValue<T>(key, out dataOut))
            {
                if(!string.IsNullOrEmpty(location))
                {
                    //TODO: error handler
                    var data = _fileHelper.GetFileContent(location);

                    if (typeof(T).IsSubclassOf(typeof(Resource)) || typeof(T) == typeof(Resource))
                    {
                        var parser = new FhirJsonParser();
                        dataOut = parser.Parse(data, typeof(T)) as T;
                    }
                    else
                    {
                        dataOut = JsonConvert.DeserializeObject<T>(data) as T;
                    }

                    // Save data in cache.
                    Set(key, dataOut, _globalExpiration);
                }
                else
                {
                    dataOut = new T();
                }
            }

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
