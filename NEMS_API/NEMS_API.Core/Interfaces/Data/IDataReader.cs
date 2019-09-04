using NEMS_API.Models.Interfaces;

namespace NEMS_API.Core.Interfaces.Data
{
    public interface IDataReader
    {
        T Read<T>(T data) where T : class, IDataItem, new();

        //TODO: search

    }
}
