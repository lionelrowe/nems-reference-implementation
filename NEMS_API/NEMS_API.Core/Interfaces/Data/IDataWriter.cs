using NEMS_API.Models.Interfaces;
using System;

namespace NEMS_API.Core.Interfaces.Data
{
    public interface IDataWriter
    {
        T Create<T>(T data) where T : IDataItem, new();

        T Create<T>(T data, DateTimeOffset lifespan) where T : IDataItem, new();

        T Create<T>(T data, string entryId, DateTimeOffset lifespan);

        //TODO: update

        void Delete<T>(T entry) where T : IDataItem;
    }
}
