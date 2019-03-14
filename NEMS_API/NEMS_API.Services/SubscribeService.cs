using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Services
{
    public class SubscribeService : ISubscribeService
    {
        private readonly IFhirValidation _fhirValidation;
        private readonly NemsApiSettings _nemsApiSettings;

        public SubscribeService(IOptions<NemsApiSettings> nemsApiSettings, IFhirValidation fhirValidation)
        {
            _fhirValidation = fhirValidation;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public async SystemTasks.Task<OperationOutcome> CreateEvent(Subscription subscription)
        {
            //## Core validation ##

            //Subscription
            var validation = _fhirValidation.ValidProfile(subscription, _nemsApiSettings.SubscriptionProfileUrl);

            if (!validation.Success)
            {
                return validation;
            }

            //## NEMS validation ##

            var customValidation = _fhirValidation.ValidSubscription(subscription);

            if (!customValidation.Success)
            {
                return customValidation;
            }

            //We are valid

            //TODO: Store subscription

            return validation;
        }
    }
}
