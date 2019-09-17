﻿using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using System.Collections.Generic;
using System.Linq;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Services
{
    public class PublishService : IPublishService
    {
        private readonly IFhirValidation _fhirValidation;
        private readonly ISchemaValidationHelper _schemaValidationHelper;
        private readonly IStaticCacheHelper _staticCacheHelper;
        private readonly IFileHelper _fileHelper;
        private readonly NemsApiSettings _nemsApiSettings;

        public PublishService(IOptions<NemsApiSettings> nemsApiSettings, IFhirValidation fhirValidation, ISchemaValidationHelper schemaValidationHelper, IStaticCacheHelper staticCacheHelper, IFileHelper fileHelper)
        {
            _fhirValidation = fhirValidation;
            _schemaValidationHelper = schemaValidationHelper;
            _staticCacheHelper = staticCacheHelper;
            _fileHelper = fileHelper;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public async SystemTasks.Task<OperationOutcome> PublishEvent(Bundle bundle)
        {
            //## Core validation ##

            //Bundle
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

            if(_nemsApiSettings.MessageHeaderValidationOnly)
            {
                return OperationOutcomeFactory.CreateOk();
            }

            //## NEMS validation ##
          
            var eventType = header.Event.Code;

            if(_nemsApiSettings.SupportedEventTypes.Count > 0 && !_nemsApiSettings.SupportedEventTypes.Contains(eventType))
            {
                var schemaMessage = $"Supplied bundle passed basic FHIR validation but event of type {eventType} is not currently supported.";

                return OperationOutcomeFactory.CreateInvalidResource("MessageHeader.event.code", schemaMessage);
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

            // ## SEND Message async and immediately return ##
            // no actual MESH stuff to handle so nothing more to do here

            return OperationOutcomeFactory.CreateOk();
        }
    }
}
