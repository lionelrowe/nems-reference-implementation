using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Services
{
    public class PatientService : IPatientService
    {
        private readonly NemsApiSettings _nemsApiSettings;
        private readonly IStaticCacheHelper _staticCacheHelper;
        private readonly IFileHelper _fileHelper;

        //TODO: Add generic db interface to switch out store
        public PatientService(IOptions<NemsApiSettings> nemsApiSettings, IStaticCacheHelper staticCacheHelper, IFileHelper fileHelper)
        {
            _nemsApiSettings = nemsApiSettings.Value;
            _staticCacheHelper = staticCacheHelper;
            _fileHelper = fileHelper;
        }

        public Bundle GetPatientBundle()
        {
            var bundle = _staticCacheHelper.GetEntry<Bundle>(CacheKeys.PatientEntries);

            if(bundle == null)
            {
                bundle = _fileHelper.GetResourceFromFile<Bundle>(_nemsApiSettings.PatientFile);

                _staticCacheHelper.AddEntry<Bundle>(bundle, CacheKeys.PatientEntries);
            }

            return bundle;
        }

        public IEnumerable<Patient> GetPatients()
        {
            var bundle = GetPatientBundle();

            return bundle?.Entry?.Where(x => x.Resource != null).Select(x => x.Resource as Patient).ToList() ?? new List<Patient>();
        }

        public Patient GetPatient(string nhsNumber)
        {
            var patients = GetPatients();

            var patient = patients.FirstOrDefault(x => x.Identifier.FirstOrDefault(y => y.System == _nemsApiSettings.ResourceUrl.NhsNumberSystem && y.Value == nhsNumber) != null);

            return patient as Patient;
        }
    }
}
