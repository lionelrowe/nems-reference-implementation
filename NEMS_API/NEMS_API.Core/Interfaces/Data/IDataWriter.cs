using NEMS_API.Models.Interfaces;

namespace NEMS_API.Core.Interfaces.Data
{
    public interface IDataWriter
    {
        T Create<T>(T data) where T : IDataItem, new();

        //TODO: update

        void Delete<T>(T entry) where T : IDataItem;
    }
}
