using Microsoft.Extensions.Options;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Services
{
    public class SdsService : ISdsService
    {
        private readonly IFileHelper _fileHelper;
        private readonly IStaticCacheHelper _staticCacheHelper;
        private readonly NemsApiSettings _nemsApiSettings;

        //TODO: Add generic db interface to switch out store
        public SdsService(IFileHelper fileHelper, IStaticCacheHelper staticCacheHelper, IOptions<NemsApiSettings> nemsApiSettings)
        {
            _fileHelper = fileHelper;
            _staticCacheHelper = staticCacheHelper;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public SdsViewModel GetFor(string asid)
        {
            var cache = GetAll();

            return cache.FirstOrDefault(x => !string.IsNullOrEmpty(asid) && x.Asid == asid);
        }

        public SdsViewModel GetFor(string odsCode, string interactionId)
        {
            //TODO: LDAP support in .net core is limited right now
            //ldapsearch - x - H ldaps://ldap.vn03.national.ncrs.nhs.uk –b "ou=services, o=nhs" 
            //"(&(nhsIDCode={odsCode}) (objectClass=nhsAS)(nhsAsSvcIA={interactionId}))"
            //uniqueIdentifier nhsMhsPartyKey

            var cache = GetAll();

            return cache.FirstOrDefault(x => !string.IsNullOrEmpty(odsCode) && x.OdsCode == odsCode
                                                    && (string.IsNullOrEmpty(interactionId) || x.Interactions.Contains(interactionId)));
        }

        public List<SdsViewModel> GetAll()
        {
            var cache = _staticCacheHelper.GetEntry<List<SdsViewModel>>(CacheKeys.SdsEntries);

            if (cache == null)
            {
                cache = _fileHelper.GetResourceFromFile<List<SdsViewModel>>(_nemsApiSettings.SdsFile);

                _staticCacheHelper.AddEntry<List<SdsViewModel>>(cache, CacheKeys.SdsEntries);
            }

            return cache;
        }
       
    }
}
