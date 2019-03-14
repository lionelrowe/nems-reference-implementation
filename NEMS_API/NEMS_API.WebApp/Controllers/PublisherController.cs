using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Interfaces.Services;

namespace NEMS_API.Controllers
{
    [Route("$process-message")]
    public class PublisherController : Controller
    {
        private readonly IPublishService _publishService;

        public PublisherController(IPublishService publishService)
        {
            _publishService = publishService;
        }

        // POST $process-message
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Resource resource)
        {
            //TODO: Remove temp code
            if (resource.ResourceType.Equals(ResourceType.OperationOutcome))
            {
                throw new HttpFhirException("Invalid Fhir Request", (OperationOutcome)resource, HttpStatusCode.BadRequest);
            }

            var published = await _publishService.PublishEvent(resource as Bundle);

            if (!published.Success)
            {
                return BadRequest(published);
            }

            return Accepted();
        }

    }
}
