using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Interfaces.Services;

namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/AppUtilities/Lists")]
    public class AppUtilsListsController : Controller
    {
        private readonly IFhirUtilities _fhirUtilities;
       
        public AppUtilsListsController(IFhirUtilities fhirUtilities)
        {
            _fhirUtilities = fhirUtilities;
        }

        [HttpGet("ValidContentTypes")]
        public async Task<IActionResult> ValidContentTypes()
        {

            var validContentTypes = _fhirUtilities.GetNemsValidContentTypes();

            return Ok(validContentTypes);
        }

        [HttpGet("EventCodes")]
        public async Task<IActionResult> EventCodes()
        {

            var eventCodes = _fhirUtilities.GetNemsEventCodes();

            return Ok(eventCodes);
        }

        //[HttpGet("Patients")]
        //public async Task<IActionResult> Patients()
        //{

        //    var patients = _patientService.GetPatientBundle();

        //    return Ok(patients);
        //}

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

    }
}
