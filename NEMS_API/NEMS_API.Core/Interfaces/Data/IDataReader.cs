using NEMS_API.Models.Interfaces;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Data
{
    public interface IDataReader
    {
        T Read<T>(T data) where T : class, IDataItem, new();

        //TODO: search
        List<T> Search<T>(T data) where T : class, IDataItem, new();

    }
}
