using NEMS_API.Core.Interfaces.Helpers;
using System.IO;

namespace NEMS_API.Core.Helpers
{
    public class FileHelper : IFileHelper
    {
        public string GetFileContent(string relativeFilePath)
        {
            var basePath = DirectoryHelper.GetBaseDirectory();

            var data = File.ReadAllText(Path.Combine(basePath, relativeFilePath));

            return data;
        }
    }
}
