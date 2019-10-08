using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;


namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/AppUtilities/Example")]
    public class AppUtilsExampleController : Controller
    {
        private readonly IFhirUtilities _fhirUtilities;
        private readonly ISdsService _sdsService;
        private readonly NemsApiSettings _nemsApiSettings;

        public AppUtilsExampleController(IOptions<NemsApiSettings> nemsApiSettings, IFhirUtilities fhirUtilities, ISdsService sdsService)
        {
            _nemsApiSettings = nemsApiSettings.Value;
            _fhirUtilities = fhirUtilities;
            _sdsService = sdsService;
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

            if (string.IsNullOrEmpty(options?.NhsNumber))
            {
                options.NhsNumber = "9912003888";
            }

            var exampleLocation = Path.Combine("Data", "examples", $"subscription-patient-{options.NhsNumber}.xml");

            var subscription = _fhirUtilities.GetResourceFromXml<Subscription>(exampleLocation) as Subscription;
            

            if (subscription == null || string.IsNullOrEmpty(options?.Asid))
            {
                return NotFound(OperationOutcomeFactory.CreateNotFound("example"));
            }

            var client = _sdsService.GetFor(options.Asid);

            if(client == null)
            {
                return NotFound(OperationOutcomeFactory.CreateNotFound(options.Asid));
            }

            var subContact = new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Url,
                Use = ContactPoint.ContactPointUse.Work,
                Value = $"{_nemsApiSettings.ResourceUrl.OrganisationReferenceUrl}{client.OdsCode}"
            };

            subscription.Contact[0] = subContact;
            subscription.Channel = new Subscription.ChannelComponent
            {
                Type = Subscription.SubscriptionChannelType.Message,
                Endpoint = client.MeshMailboxId
            };
            

            return Ok(subscription);
        }

        [HttpPost("Convert")]
        public async Task<IActionResult> Convert([FromBody] Resource resource)
        {
            return Ok(resource);
        }

    }
}
