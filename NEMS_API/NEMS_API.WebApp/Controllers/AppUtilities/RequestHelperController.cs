using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Core.Resources;
using NEMS_API.Models.Core;


namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/AppUtilities/RequestHelper")]
    public class AppUtilsRequestHelperController : Controller
    {
        private readonly NemsApiSettings _nemsApiSettings;

        private readonly ISdsService _sdsService;

        public AppUtilsRequestHelperController(IOptions<NemsApiSettings> nemsApiSettings,  ISdsService sdsService)
        {
            _sdsService = sdsService;
            _nemsApiSettings = nemsApiSettings.Value;
        }
        
        [HttpGet("GenerateJwtToken")]
        public IActionResult GenerateJwtToken([FromQuery] JwtRequest jwtRequest)
        {

            var jwt = JwtFactory.Generate(jwtRequest.Hydrate());

            return Ok(jwt);
        }

        [HttpGet("GetDefaultPublisherSystem")]
        public IActionResult GetDefaultPublisherSystem()
        {
            //TODO: dynamic publisher based on selected event type
            var system = _sdsService.GetAll().FirstOrDefault(x => x.Interactions.Contains(FhirConstants.IIPublishEvent("pds-change-of-gp-1")));

            return Ok(system);
        }

        [HttpGet("GetSubscriberSystems")]
        public IActionResult GetSubscriberSystems()
        {
            var system = _sdsService.GetAll().Where(x => x.Interactions.Contains(FhirConstants.IISubscriptionCreate));

            return Ok(system);
        }

        [HttpGet("GetSpineSystem")]
        public IActionResult GetSpineSystem()
        {

            var system = new SdsViewModel
            {
                Asid = _nemsApiSettings.SpineASID
            };

            return Ok(system);
        }

    }
}
