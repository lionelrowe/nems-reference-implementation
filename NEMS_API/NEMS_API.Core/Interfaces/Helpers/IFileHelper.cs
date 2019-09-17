namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IFileHelper
    {
        string GetStringFromFile(string relativeFilePath);

        T GetResourceFromFile<T>(string relativeFilePath, bool isXml = false) where T : class;
    }
}
