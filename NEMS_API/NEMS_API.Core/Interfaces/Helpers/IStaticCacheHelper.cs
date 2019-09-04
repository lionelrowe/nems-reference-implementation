using NEMS_API.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IStaticCacheHelper
    {
        T GetDataItem<T>(string key, string location = null) where T : class, new();

        List<T> GetDataList<T>(string key, string location = null) where T : class, new();

        T AddItem<T>(T entry) where T : IDataItem, new();
        //Te AddCacheData<Tc, Te>(string key, Te entry) where Te : class, new() where Tc : class, new();

        void Set<T>(string key, T value, DateTimeOffset absoluteExpiration);

        void RemoveItem<T>(T entry) where T : IDataItem;

        void RemoveAll(string cacheKey);
    }
}
