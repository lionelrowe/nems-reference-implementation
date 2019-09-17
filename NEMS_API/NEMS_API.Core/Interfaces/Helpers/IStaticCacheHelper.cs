using NEMS_API.Models.Interfaces;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IStaticCacheHelper
    {
        T GetEntry<T>(string key) where T : class, new();

        //List<T> GetDataList<T>(string key) where T : class, new();

        T AddEntry<T>(T entry) where T : IDataItem;

        T AddEntry<T>(T entry, string cacheKey);

        T AddListItem<T>(T entry) where T : IDataItem, new();
        //Te AddCacheData<Tc, Te>(string key, Te entry) where Te : class, new() where Tc : class, new();

        //void Set<T>(string key, T value, DateTimeOffset absoluteExpiration);

        void RemoveEntry(string cacheKey);

        void RemoveListItem<T>(T entry) where T : IDataItem;
    }
}
