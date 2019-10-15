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
        private readonly IPublishService _publishService;
        private readonly IMessageExchangeService _messageExchangeService;

        public AppUtilsSubscriptionController(ISubscribeService subscribeService, IPublishService publishService, IMessageExchangeService messageExchangeService)
        {
            _subscribeService = subscribeService;
            _publishService = publishService;
            _messageExchangeService = messageExchangeService;
        }

        [HttpGet("{asid:long}")]
        public IActionResult GetSubscriptionsByAsid(long asid)
        {
            var request = FhirRequest.Create(null, null, null, $"{asid}");

            var subscriptions = _subscribeService.SearchEvent(request);

            var basePath = $"{Request.Scheme}://{Request.Host}";

            var subscriptionPath = $"{basePath}/nems-ri/STU3/Subscription";

            var bundle = FhirHelper.GetAsSearchset(subscriptions, subscriptionPath, $"{basePath}{Request.Path}");

            return Ok(bundle);
        }

        [HttpGet("MessageMatch/{messageId}")]
        public IActionResult GetSubscribersOfMessage(string messageId)
        {
            var bundle = _messageExchangeService.GetMessage(messageId);

            if(bundle == null)
            {
                return NotFound($"Event Message with id {messageId} was not found.");
            }

            var criteria = _publishService.GetSubscriptionMatchingCriteria(bundle);

            var subscriberMailboxes = _subscribeService.SubscriptionMatcher(criteria);

            return Ok(subscriberMailboxes);
        }

    }
}
