using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly IStaticCacheHelper _staticCacheHelper;
        private readonly NemsApiSettings _nemsApiSettings;

        //TODO: Add generic db interface to switch out store
        public SdsService(IMemoryCache cache, IStaticCacheHelper staticCacheHelper, IOptions<NemsApiSettings> nemsApiSettings)
        {
            _cache = cache;
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
            var cache = _staticCacheHelper.GetDataList<SdsViewModel>(CacheKeys.SdsEntries, _nemsApiSettings.SdsFile);

            return cache;
        }
       
    }
}
