using System;
using System.Collections.Generic;
using System.Text;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IFileHelper
    {
        string GetFileContent(string relativeFilePath);
    }
}
