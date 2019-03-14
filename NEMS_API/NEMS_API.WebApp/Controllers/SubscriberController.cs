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

        /// <summary>
        /// Gets a resource by the supplied id.
        /// </summary>
        /// <returns>A FHIR Resource</returns>
        /// <response code="200">Returns the FHIR Resource</response>
        [ProducesResponseType(typeof(Resource), 200)]
        [HttpGet("{subscriptionId}")]
        public async Task<IActionResult> Read(string subscriptionId)
        {

            //var result = await someService.Get(subscriptionId);

            //TODO: check if 404 or other
            //if (result.ResourceType == ResourceType.OperationOutcome)
            //{
            //    return NotFound(result);
            //}

            return Ok("result");
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

        /// <summary>
        /// Deletes a record that was previously persisted into a datastore.
        /// </summary>
        /// <returns>The OperationOutcome</returns>
        /// <response code="200">Returns OperationOutcome</response>
        [HttpDelete("{subscriptionId}")]
        public async Task<IActionResult> Delete(string subscriptionId)
        {
            //var result = await someService.Get(subscriptionId);

            //if (result != null && result.Success)
            //{
                //Assume success
                return Ok();
            //}

            //TODO: check if 404 or other
            //return NotFound(result);
        }

    }
}
