using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Core.Resources;
using NEMS_API.Models.Core;
using NEMS_API.WebApp.Core.Filters;
using NEMS_API.WebApp.Core.Filters.Attributes;

namespace NEMS_API.Controllers
{
    [Route("nems-ri/STU3/Events/1/")]
    public class PublisherController : Controller
    {
        private readonly IPublishService _publishService;
        private readonly ISubscribeService _subscribeService;
        private readonly NemsApiSettings _nemsApiSettings;

        public PublisherController(IOptions<NemsApiSettings> nemsApiSettings, IPublishService publishService, ISubscribeService subscribeService)
        {
            _publishService = publishService;
            _subscribeService = subscribeService;
            _nemsApiSettings = nemsApiSettings.Value;
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
            var request = FhirRequest.Create(null, resource, null, RequestingAsid());

            var published = await _publishService.ValidateEvent(request);   

            if (published.ResourceType == ResourceType.Bundle)
            {
                var bundle = published as Bundle;

                var criteria = _publishService.GetSubscriptionMatchingCriteria(bundle);

                var subscriberMailboxes = _subscribeService.SubscriptionMatcher(criteria);

                _publishService.PublishEvent(bundle, subscriberMailboxes, criteria.ActiveEventType.WorkflowID);

                HttpContext.Response.Headers.Add("x-EventMessageId", bundle.Id);

                return Accepted();
            }

            //TODO: depreciation warning

            
            return BadRequest(published);
        }

        private string RequestingAsid()
        {
            if (Request.Headers.ContainsKey(FhirConstants.HeaderFromAsid))
            {
                return Request.Headers[FhirConstants.HeaderFromAsid];
            }

            //This should never be null as it's checked in the middleware
            return null;
        }


    }
}
