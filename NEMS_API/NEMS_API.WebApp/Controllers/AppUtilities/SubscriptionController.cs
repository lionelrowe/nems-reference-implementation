using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Core.Helpers;

namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/AppUtilities/Subscription")]
    public class AppUtilsSubscriptionController : Controller
    {
        private readonly ISubscribeService _subscribeService;
        
        public AppUtilsSubscriptionController(ISubscribeService subscribeService)
        {
            _subscribeService = subscribeService;
        }

        [HttpGet("{asid}")]
        public IActionResult GetSubscriptionsByAsid(string asid)
        {
            var request = FhirRequest.Create(null, null, null, asid);

            var subscriptions = _subscribeService.SearchEvent(request);

            var basePath = $"{Request.Scheme}://{Request.Host}";

            var subscriptionPath = $"{basePath}/nems-ri/STU3/Subscription";

            var bundle = FhirHelper.GetAsSearchset(subscriptions, subscriptionPath, $"{basePath}{Request.Path}");

            return Ok(bundle);
        }

    }
}
