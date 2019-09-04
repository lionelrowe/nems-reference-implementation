using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.FhirResources;


namespace NEMS_API.Controllers
{
    [Route("nems-ri/AppUtilities")]
    public class AppUtilitiesController : Controller
    {
        private readonly NemsApiSettings _nemsApiSettings;

        private readonly IPatientService _patientService;

        private readonly IFhirUtilities _fhirUtilities;

        private readonly ISdsService _sdsService;

        public AppUtilitiesController(IOptions<NemsApiSettings> nemsApiSettings,  IPatientService patientService, IFhirUtilities fhirUtilities, ISdsService sdsService)
        {
            _patientService = patientService;
            _fhirUtilities = fhirUtilities;
            _sdsService = sdsService;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        [HttpGet("Example/Publish")]
        public async Task<IActionResult> ExamplePublish([FromQuery] PublishExampleOptions options)
        {

            var patient = _patientService.GetPatient(options.NhsNumber);

            var evetType = _fhirUtilities.GetNemsEventCode(options.EventMessageTypeId);

            var healthCareService = new NemsHealthCareService
            {
                Id = Guid.NewGuid()
            };

            var communication = new NemsCommunication
            {
                Id = Guid.NewGuid(),
                Patient = patient
            };

            var header = new NemsMessageHeader(_nemsApiSettings.ResourceUrl)
            {
                Id = Guid.NewGuid(),
                EvetType = evetType,
                MainFocusId = communication.Id
            };

            var bundle = new NemsBundle
            {
                Id = Guid.NewGuid()
            };

            bundle.AddEntry(header.Id, header.Default());

            bundle.AddEntry(healthCareService.Id, healthCareService.Default());

            bundle.AddEntry(communication.Id, communication.Default());

            bundle.AddEntry(patient.Id, patient);

            var example = bundle.Default();

            return Ok(example);
        }

        [HttpGet("Lists/ValidContentTypes")]
        public async Task<IActionResult> ValidContentTypes()
        {

            var validContentTypes = _fhirUtilities.GetNemsValidContentTypes();

            return Ok(validContentTypes);
        }

        [HttpGet("Lists/EventCodes")]
        public async Task<IActionResult> EventCodes()
        {

            var eventCodes = _fhirUtilities.GetNemsEventCodes();

            return Ok(eventCodes);
        }

        [HttpGet("Patients")]
        public async Task<IActionResult> Patients()
        {

            var patients = _patientService.GetPatientBundle();

            return Ok(patients);
        }

        //[HttpGet("Patients/{nhsNumber:regex(^[[0-9]]{{10}}$)}")]
        //public async Task<IActionResult> Patient(string nhsNumber)
        //{

        //    var patient = _patientService.GetPatient(nhsNumber);

        //    if (patient == null)
        //    {
        //        return NotFound(OperationOutcomeFactory.CreateNotFound(nhsNumber));
        //    }

        //    return Ok(patient);
        //}

        [HttpGet("Systems/DefaultPublisher")]
        public async Task<IActionResult> Systems_DefaultPublisher()
        {

            var system = _sdsService.GetAll().FirstOrDefault(x => x.Interactions.Contains("urn:nhs:names:services:clinicals-sync:PublicationApiPost"));

            return Ok(system);
        }

    }
}
