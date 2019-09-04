using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;
using System.Net;

namespace NEMS_API.Services
{
    public class SubscribeService : ISubscribeService
    {
        private readonly IFhirValidation _fhirValidation;
        private readonly NemsApiSettings _nemsApiSettings;
        private readonly IDataWriter _dataWriter;
        private readonly IDataReader _dataReader;

        public SubscribeService(IOptions<NemsApiSettings> nemsApiSettings, IFhirValidation fhirValidation, IDataWriter dataWriter, IDataReader dataReader)
        {
            _fhirValidation = fhirValidation;
            _nemsApiSettings = nemsApiSettings.Value;
            _dataWriter = dataWriter;
            _dataReader = dataReader;
        }

        public Subscription ReadEvent(string id)
        {
             var entry = ReadEventAsNems(id);

            return NemsSubscription.ToSubscription(entry);
        }

        public Resource CreateEvent(Subscription subscription)
        {
            //## Core validation ##

            //Subscription
            var validation = _fhirValidation.ValidProfile(subscription, _nemsApiSettings.ResourceUrl.SubscriptionProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            //## NEMS validation ##
            var nemsSubscription = new NemsSubscription(subscription);

            var customValidation = _fhirValidation.ValidSubscription(nemsSubscription);

            if (!customValidation.Success)
            {
                return customValidation;
            }

            //We are valid

            //TODO: Store subscription
            nemsSubscription.SetMeta();
            var entry = _dataWriter.Create(nemsSubscription);

            return entry;
        }

        public void DeleteEvent(string id)
        {
            var subscription = ReadEventAsNems(id);

            try
            {
                _dataWriter.Delete(subscription);
            }
            catch
            {
                throw new HttpFhirException("Internal Error [DeleteEvent]", OperationOutcomeFactory.CreateInternalError("Unknown Internal Error Encountered."), HttpStatusCode.InternalServerError);
            }
            
        }

        private NemsSubscription ReadEventAsNems(string id)
        {
            //TODO: Error checking and throw error

            var item = new NemsSubscription
            {
                Id = id
            };

            var entry = _dataReader.Read(item);

            if (entry == null)
            {
                throw new HttpFhirException("Event Not Found", OperationOutcomeFactory.CreateNotFound(id), HttpStatusCode.NotFound);
            }

            return entry;
        }
    }
}
