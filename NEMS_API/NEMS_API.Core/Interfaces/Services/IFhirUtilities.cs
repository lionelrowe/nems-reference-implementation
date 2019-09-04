using NEMS_API.Models.Core;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IFhirUtilities
    {
        IEnumerable<KeyValuePair<string, string>> GetNemsEventCodes();

        KeyValuePair<string, string> GetNemsEventCode(string code);

        IEnumerable<KvList> GetNemsValidContentTypes();
    }
}
