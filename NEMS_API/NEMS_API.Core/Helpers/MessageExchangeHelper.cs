using Microsoft.AspNetCore.SignalR;
using NEMS_API.Core.Hubs;
using NEMS_API.Core.Interfaces.Helpers;


namespace NEMS_API.Core.Helpers
{
    public class MessageExchangeHelper : IMessageExchangeHelper
    {
        private readonly IHubContext<PublisherHub> _hubContext;

        public MessageExchangeHelper(IHubContext<PublisherHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async void SyncRequest(string workflowId)
        {
            await _hubContext.Clients.All.SendAsync($"syncMailbox_{workflowId}");
        }
    }
}
