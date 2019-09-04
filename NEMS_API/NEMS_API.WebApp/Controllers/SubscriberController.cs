using System.Net;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.FhirResources;
using NEMS_API.WebApp.Core.Filters;

namespace NEMS_API.Controllers
{
    [Route("nems-ri/STU3/Subscription")]
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
            var subscription = _subscribeService.ReadEvent(subscriptionId);

            return Ok(subscription);
        }

        // POST /Subscription
        [FhirFormatterValidation]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Resource resource)
        {
            if (!resource.ResourceType.Equals(ResourceType.Subscription))
            {
                throw new HttpFhirException("Invalid Fhir Request", OperationOutcomeFactory.CreateInvalidResourceType(resource.ResourceType.ToString()), HttpStatusCode.BadRequest);
            }

            var create = _subscribeService.CreateEvent(resource as Subscription);

            //TODO: depreciation warning

            if (create is NemsSubscription subscription)
            {
                var resourceUri = $"{Request.Scheme}://{Request.Host}/nems-ri/STU3/Subscription/{subscription.Id}";
                return Created(resourceUri, null);
            }

            return BadRequest(create);
        }

        /// <summary>
        /// Deletes a record that was previously persisted into a datastore.
        /// </summary>
        /// <returns>EMpty ok</returns>
        /// <response code="200">Returns empty</response>
        [HttpDelete("{subscriptionId}")]
        public async Task<IActionResult> Delete(string subscriptionId)
        {
            _subscribeService.DeleteEvent(subscriptionId);

            return Ok();
        }

    }
}
