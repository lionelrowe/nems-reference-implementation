using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;
using System.Linq;
using System.Net;

namespace NEMS_API.Services
{
    public class SubscribeService : ISubscribeService
    {
        private readonly ISdsService _sdsService;
        private readonly IFhirValidation _fhirValidation;
        private readonly NemsApiSettings _nemsApiSettings;
        private readonly IDataWriter _dataWriter;
        private readonly IDataReader _dataReader;

        public SubscribeService(IOptions<NemsApiSettings> nemsApiSettings, ISdsService sdsService, IFhirValidation fhirValidation, IDataWriter dataWriter, IDataReader dataReader)
        {
            _sdsService = sdsService;
            _fhirValidation = fhirValidation;
            _nemsApiSettings = nemsApiSettings.Value;
            _dataWriter = dataWriter;
            _dataReader = dataReader;
        }

        public Subscription ReadEvent(FhirRequest request)
        {
             var entry = ReadEventAsNems(request);

            return NemsSubscription.ToSubscription(entry);
        }

        public Resource CreateEvent(FhirRequest request)
        {
            //## Core validation ##
            var subscription = request.Resource as Subscription;

            //Subscription
            var validation = _fhirValidation.ValidProfile(subscription, _nemsApiSettings.ResourceUrl.SubscriptionProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            //## NEMS validation ##

            //This should never be null as it's checked in the middleware
            var cache = GetClientCache(request.RequestingAsid);

            var nemsSubscription = new NemsSubscription(subscription);
            nemsSubscription.RequesterOdsCode = cache.OdsCode;
            nemsSubscription.RequesterAsid = request.RequestingAsid;

            var customValidation = _fhirValidation.ValidSubscription(nemsSubscription);

            if (!customValidation.Success)
            {

                if(customValidation.Issue.FirstOrDefault(x => x.Details.Coding.First().Code == "ACCESS_DENIED") != null)
                {
                    throw new HttpFhirException("Subscription asid mismatch", OperationOutcomeFactory.CreateAccessDenied(), HttpStatusCode.Forbidden);
                }

                return customValidation;
            }

            //We are valid

            nemsSubscription.SetMeta();
            var entry = _dataWriter.Create(nemsSubscription);

            return entry;
        }

        public void DeleteEvent(FhirRequest request)
        {
            var subscription = ReadEventAsNems(request);

            try
            {
                _dataWriter.Delete(subscription);
            }
            catch
            {
                throw new HttpFhirException("Internal Error [DeleteEvent]", OperationOutcomeFactory.CreateInternalError("Unknown Internal Error Encountered."), HttpStatusCode.InternalServerError);
            }
            
        }

        private NemsSubscription ReadEventAsNems(FhirRequest request)
        {
            var item = new NemsSubscription
            {
                Id = request.Id
            };

            var entry = _dataReader.Read(item);

            if (entry == null)
            {
                throw new HttpFhirException("Event Not Found", OperationOutcomeFactory.CreateNotFound(request.Id), HttpStatusCode.NotFound);
            }

            //This should never be null as it's checked in the middleware
            var cache = GetClientCache(request.RequestingAsid);

            if (entry.RequesterAsid != cache.Asid)
            {
                throw new HttpFhirException("Subscription asid mismatch", OperationOutcomeFactory.CreateAccessDenied(), HttpStatusCode.Forbidden);
            }

            return entry;
        }

        private SdsViewModel GetClientCache(string fromAsid)
        {
            var cache = _sdsService.GetFor(fromAsid);

            return cache;
        }
    }
}
