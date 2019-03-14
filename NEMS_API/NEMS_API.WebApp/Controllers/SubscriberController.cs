using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Services;

namespace NEMS_API.Controllers
{
    [Route("Subscription")]
    public class SubscriberController : Controller
    {
        private readonly ISubscribeService _subscribeService;

        public SubscriberController(ISubscribeService subscribeService)
        {
            _subscribeService = subscribeService;
        }

        // POST /Subscription
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Resource resource)
        {
            //TODO: Remove temp code
            if (resource.ResourceType.Equals(ResourceType.OperationOutcome))
            {
                throw new HttpFhirException("Invalid Fhir Request", (OperationOutcome)resource, HttpStatusCode.BadRequest);
            }

            if (!resource.ResourceType.Equals(ResourceType.Subscription))
            {
                throw new HttpFhirException("Invalid Fhir Request", OperationOutcomeFactory.CreateInvalidResourceType(resource.ResourceType.ToString()), HttpStatusCode.BadRequest);
            }

            var create = await _subscribeService.CreateEvent(resource as Subscription);

            if (!create.Success)
            {
                return BadRequest(create);
            }

            return Created("TODO:url", null);
        }

    }
}
