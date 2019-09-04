using Microsoft.Extensions.Options;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Services
{
    public class FhirUtilities : IFhirUtilities
    {
        private readonly NemsApiSettings _nemsApiSettings;
        private readonly IFhirResourceHelper _fhirResourceHelper;

        public FhirUtilities(IOptions<NemsApiSettings> nemsApiSettings, IFhirResourceHelper fhirResourceHelper)
        {
            _nemsApiSettings = nemsApiSettings.Value;
            _fhirResourceHelper = fhirResourceHelper;
        }

        public IEnumerable<KeyValuePair<string, string>> GetNemsEventCodes()
        {
            var codeSystem = _fhirResourceHelper.GetCodeSystem(_nemsApiSettings.ResourceUrl.EventCodesUrl);

            return codeSystem?.Concept?.Select(x => new KeyValuePair<string, string>(x.Code, x.Display))?.ToList() ?? new List<KeyValuePair<string, string>>();
        }

        public KeyValuePair<string, string> GetNemsEventCode(string code)
        {
            var codes = GetNemsEventCodes();

            return codes.FirstOrDefault(x => x.Key == code);
        }

        public IEnumerable<KvList> GetNemsValidContentTypes()
        {
            return _nemsApiSettings.SupportedContentTypes;
        }
    }
}
