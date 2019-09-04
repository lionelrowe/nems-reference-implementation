using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.WebApp.Core.Filters;
using NEMS_API.WebApp.Core.Filters.Attributes;

namespace NEMS_API.Controllers
{
    [Route("nems-ri/STU3/Events/1/")]
    public class PublisherController : Controller
    {
        private readonly IPublishService _publishService;

        public PublisherController(IPublishService publishService)
        {
            _publishService = publishService;
        }

        /// <summary>
        /// Publish an event via the NEMS
        /// </summary>
        /// <param name="resource">An EMS-Bundle-1 FHIR Bundle</param>
        /// <returns>Empty Result</returns>
        /// <response code="202">Returns Empty Result</response>
        /// <response code="400">FHIR Operation Outcome</response> 
        [ProducesResponseType(202)]
        [ProducesResponseType(typeof(OperationOutcome), 400)]
        [SwaggerParameterContentType(name: "resource", contentType: "application/fhir+xml", description: "A EMS-Bundle-1 FHIR Bundle", exampleUrl: "examples/nems-bundle-1-example.xml", required: true)]
        [SwaggerParameterContentType(name: "resource", contentType: "application/fhir+json", description: "A EMS-Bundle-1 FHIR Bundle", exampleUrl: "examples/nems-bundle-1-example.json", required: true)]
        [FhirFormatterValidation]
        [HttpPost("$process-message")]
        public async Task<IActionResult> Post([FromBody] Resource resource)
        {
            var published = await _publishService.PublishEvent(resource as Bundle);

            if (!published.Success)
            {
                return BadRequest(published);
            }

            //TODO: depreciation warning

            return Accepted();
        }

    }
}
