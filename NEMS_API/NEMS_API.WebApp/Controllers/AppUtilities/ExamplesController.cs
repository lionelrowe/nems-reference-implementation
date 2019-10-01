using System.IO;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;


namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/AppUtilities/Example")]
    public class AppUtilsExampleController : Controller
    {
        private readonly IFhirUtilities _fhirUtilities;

        public AppUtilsExampleController(IFhirUtilities fhirUtilities)
        {
            _fhirUtilities = fhirUtilities;
        }

        [HttpGet("Publish")]
        public async Task<IActionResult> Publish([FromQuery] ExampleOptions options)
        {

            var evetType = _fhirUtilities.GetNemsEventCode(options.EventMessageTypeId);

            var exampleLocation = Path.Combine("Data", "examples", $"{evetType.Key}.xml");

            var example = _fhirUtilities.GetResourceFromXml<Bundle>(exampleLocation);

            return Ok(example);
        }

        [HttpGet("Subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] ExampleOptions options)
        {
            Resource example = null;

            if (!string.IsNullOrEmpty(options?.NhsNumber))
            {
                var exampleLocation = Path.Combine("Data", "examples", $"subscription-patient-{options.NhsNumber}.xml");

                example = _fhirUtilities.GetResourceFromXml<Subscription>(exampleLocation);
            }

            if (example == null)
            {
                return NotFound(OperationOutcomeFactory.CreateNotFound("example"));
            }

            return Ok(example);
        }

    }
}
