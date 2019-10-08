using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Core.Resources;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;
using NEMS_API.Models.MessageExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Services
{
    public class PublishService : IPublishService
    {
        private readonly IFhirValidation _fhirValidation;
        private readonly ISchemaValidationHelper _schemaValidationHelper;
        private readonly IStaticCacheHelper _staticCacheHelper;
        private readonly IFileHelper _fileHelper;
        private readonly ISdsService _sdsService;
        private readonly IMessageExchangeHelper _messageExchangeHelper;
        private readonly IDataReader _dataReader;
        private readonly IDataWriter _dataWriter;
        private readonly NemsApiSettings _nemsApiSettings;
        private DateTimeOffset _eventStoreExpiration = new DateTimeOffset(DateTime.UtcNow.AddHours(3));

        public PublishService(IOptions<NemsApiSettings> nemsApiSettings, IFhirValidation fhirValidation, ISchemaValidationHelper schemaValidationHelper, 
            IStaticCacheHelper staticCacheHelper, IFileHelper fileHelper, ISdsService sdsService, IMessageExchangeHelper messageExchangeHelper, IDataReader dataReader, IDataWriter dataWriter)
        {
            _fhirValidation = fhirValidation;
            _schemaValidationHelper = schemaValidationHelper;
            _staticCacheHelper = staticCacheHelper;
            _fileHelper = fileHelper;
            _sdsService = sdsService;
            _messageExchangeHelper = messageExchangeHelper;
            _dataReader = dataReader;
            _dataWriter = dataWriter;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public async SystemTasks.Task<OperationOutcome> PublishEvent(FhirRequest request)
        {
            //## Core validation ##

            //Bundle
            var bundle = request.Resource as Bundle;
            var basicValidationOnly = bundle.Meta?.Profile?.Contains(_nemsApiSettings.ValidationOptions.ValidateMessageFromProfileUrl) ?? _nemsApiSettings.ValidationOptions.ValidateMessageHeaderOnly;
            var meta = bundle.Meta;
            bundle.Meta = meta ?? new Meta();
            bundle.Meta.Profile = new List<string> { _nemsApiSettings.ResourceUrl.BundleProfileUrl }; //reset profile

            var validation = _fhirValidation.ValidProfile(bundle, null);

            if (!validation.Success)
            {
                return validation;
            }

            //MessageHeader
            var header = FhirHelper.GetFirstEntryOfTypeProfile<MessageHeader>(bundle, _nemsApiSettings.ResourceUrl.HeaderProfileUrl);

            if(header == null)
            {
                return OperationOutcomeFactory.CreateInvalidResource("MessageHeader", "Entry of resource type MessageHeader must be the first entry.");
            }

            //Assuming messageheader does not contain the right profile value
            validation = _fhirValidation.ValidProfile(header, _nemsApiSettings.ResourceUrl.HeaderProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            if(basicValidationOnly)
            {
                return OperationOutcomeFactory.CreateOk();
            }

            //## NEMS validation ##
          
            var eventType = header.Event.Code;
            var activeEventType = _nemsApiSettings.SupportedEventTypes.FirstOrDefault(e => e.Name == eventType);

            var client = _sdsService.GetFor(request.RequestingAsid);

            if (client == null)
            {
                throw new HttpFhirException("Invalid/Missing Header", OperationOutcomeFactory.CreateInvalidHeader("fromASID", null), HttpStatusCode.BadRequest);
            }

            var eventTypeInteractionId = FhirConstants.IIPublishEvent(eventType);

            if (!client.Interactions.Contains(eventTypeInteractionId))
            {
                throw new HttpFhirException("Publisher asid does not have access to perform this Interaction", OperationOutcomeFactory.CreateAccessDenied(), HttpStatusCode.Forbidden);
            }

            if (_nemsApiSettings.ValidationOptions.AcceptSupportedEventsOnly && _nemsApiSettings.SupportedEventTypes.Count > 0 && activeEventType == null)
            {
                var schemaMessage = $"Supplied bundle passed basic FHIR validation but event of type {eventType} is not currently supported.";

                return OperationOutcomeFactory.CreateInvalidResource("MessageHeader.event.code", schemaMessage);
            }

            if(!_nemsApiSettings.ValidationOptions.AcceptSupportedEventsOnly && activeEventType == null)
            {
                activeEventType = new EventTypeConfig
                {
                    Name = "Generic-Event-Type-1",
                    WorkflowID = "GENERICEVENTTYPE_1"
                };
            }

            var schemaMessages = new List<string>();

            var eventTypeSchemas = _nemsApiSettings.BundleValidationSchemas.FirstOrDefault(x => x.Key == eventType).Value;

            foreach ((KvList typeSchema, int i) in eventTypeSchemas.Select((value, i) => (value, i)))
            {
                var resource = bundle.Entry.FirstOrDefault(x => x.Resource.ResourceType.ToString() == typeSchema.Key)?.Resource;

                if (resource == null)
                {
                    schemaMessages.Add($"Expecting Bundle entry of type {typeSchema.Key} but found none.");
                    break;
                }

                var schema = new SchemaProfile
                {
                    CacheKeyType = $"{eventType}:{typeSchema.Key}",
                    SchemaPathBase = $"Bundle.entry[{i}].{typeSchema.Key}"
                };

                var schemaEntry = _staticCacheHelper.GetEntry<SchemaProfile>(schema.CacheKey) ?? schema;

                if (string.IsNullOrEmpty(schemaEntry.Data))
                {
                    var schemaFile = $"Data/rules/{eventType}.{typeSchema.Key}.schema.json";
                    var schemaJson = _fileHelper.GetStringFromFile(schemaFile);

                    schemaEntry.Data = schemaJson;

                    _staticCacheHelper.AddEntry<SchemaProfile>(schemaEntry);
                }

                var schemaValidation = _schemaValidationHelper.ValidateFhirResource(resource, schemaEntry);

                if(schemaValidation != null)
                {
                    schemaMessages.AddRange(schemaValidation);
                }
            }

            if(schemaMessages.Count > 0)
            {
                return OperationOutcomeFactory.CreateInvalidResource("Bundle", schemaMessages);
            }

            //We are valid
            bundle.Id = Guid.NewGuid().ToString();

            _dataWriter.Create(bundle, CacheKeys.NemsEventEntry(bundle.Id), _eventStoreExpiration);

            var pdsExtension = header.GetExtension(_nemsApiSettings.ResourceUrl.ExtensionRoutingDemographicsUrl);
            var patientIdentifier = pdsExtension.GetExtensionValue<Identifier>("nhsNumber");
            var patientDob = pdsExtension.GetExtensionValue<FhirDateTime>("birthDateTime"); //.Extension.FirstOrDefault(e => e.Url == "birthDateTime").Value as FhirDateTime;

            SendToMailboxes($"{patientIdentifier.System}|{patientIdentifier.Value}", patientDob, activeEventType, bundle.Id);

            _messageExchangeHelper.SyncRequest(activeEventType.WorkflowID);

            return OperationOutcomeFactory.CreateOk();
        }

        private void SendToMailboxes(string patientIdentifier, FhirDateTime birthDate, EventTypeConfig activeEventType, string id)
        {
            var workflowSubscribers = _sdsService.GetAll().Where(x => x.WorkflowIds.Contains(activeEventType.WorkflowID));

            var ageDt = DateTime.Parse(birthDate.Value);

            var now = DateTime.UtcNow;

            var age = now.Year - ageDt.Year;

            // leap year check
            if (ageDt > now.AddYears(-age))
            {
                age--;
            }

            foreach (var sub in workflowSubscribers)
            {
                var subscriptions = _dataReader.Search(new NemsSubscription());

                var clientSubscriptions = subscriptions.Where(x => x.RequesterAsid == sub.Asid && ( _nemsApiSettings.ValidationOptions.SkipSubscriptionMatching || (x.CriteriaModel.PatientIdentifier == patientIdentifier.ToUpperInvariant() && x.CriteriaModel.MessageHeaderEvents.Contains(activeEventType.Name.ToUpperInvariant()) && x.CriteriaModel.ValidAge(age))));


                if (clientSubscriptions.Any())
                {
                    var dataItem = new InboxRecordIdentifier
                    {
                        Id = id,
                        Data = sub.MeshMailboxId
                    };

                    _dataWriter.Create(dataItem, _eventStoreExpiration);
                }

            }
        }
    }
}
