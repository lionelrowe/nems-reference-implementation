using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Services
{
    public class PublishService : IPublishService
    {
        private readonly IFhirValidation _fhirValidation;
        private readonly NemsApiSettings _nemsApiSettings;

        public PublishService(IOptions<NemsApiSettings> nemsApiSettings, IFhirValidation fhirValidation)
        {
            _fhirValidation = fhirValidation;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public async SystemTasks.Task<OperationOutcome> PublishEvent(Bundle bundle)
        {
            //## Core validation ##

            //Bundle
            var validation = _fhirValidation.ValidProfile(bundle, _nemsApiSettings.BundleProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            //Header
            var header = FhirHelper.GetFirstEntryOfTypeProfile<MessageHeader>(bundle, _nemsApiSettings.HeaderProfileUrl);

            if(header == null)
            {
                return OperationOutcomeFactory.CreateInvalidResource("MessageHeader", "Entry of resource type MessageHeader must be the first entry.");
            }

            if (string.IsNullOrEmpty(header.Source.Name))
            {
                return OperationOutcomeFactory.CreateInvalidResource("MessageHeader.Source.Name", "Name cannot be null.");
            }

            validation = _fhirValidation.ValidProfile(header, _nemsApiSettings.HeaderProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            //## NEMS validation ##

            //TODO: validate other entries within the bundle?

            //We are valid

            // ## SEND Message async and immediately return ##
            // no actual MESH stuff to handle so nothing more to do here
            // we can mimic ITK3 message in subscriber controller

            return OperationOutcomeFactory.CreateOk();
        }
    }
}
