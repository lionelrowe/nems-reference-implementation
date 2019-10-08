using Microsoft.AspNetCore.Mvc;
using NEMS_API.Core.Interfaces.Services;


namespace NEMS_API.Controllers.AppUtilities
{
    [Route("nems-ri/MessageExchange")]
    public class MessageExchangeController : Controller
    {
        private readonly IMessageExchangeService _messageExchangeService;

        public MessageExchangeController(IMessageExchangeService messageExchangeService)
        {
            _messageExchangeService = messageExchangeService;
        }

        [HttpGet("{mailboxId}/inbox")]
        public IActionResult CheckInbox(string mailboxId)
        {

            var inboxRecord = _messageExchangeService.CheckMessages(mailboxId);

            return Ok(inboxRecord);

            //TODO: 403
        }

        [HttpGet("{mailboxId}/inbox/{messageId}")]
        public IActionResult DownloadMessage(string mailboxId, string messageId)
        {

            var entry = _messageExchangeService.GetMessage(messageId);

            if(entry == null)
            {
                return NotFound();
            }

            return Ok(entry);

            //TODO: 206
            //TODO: 403
            //TODO: 410
        }

        [HttpPut("{mailboxId}/inbox/{messageId}/status/acknowledged")]
        public IActionResult AcknowledgedDownload(string mailboxId, string messageId)
        {
            _messageExchangeService.RemoveMessage(mailboxId, messageId);

            return Ok();

            //TODO: 403
        }


    }
}
