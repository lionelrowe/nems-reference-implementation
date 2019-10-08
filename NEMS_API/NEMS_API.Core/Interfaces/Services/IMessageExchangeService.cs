using Hl7.Fhir.Model;
using NEMS_API.Models.MessageExchange;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IMessageExchangeService
    {
        InboxRecord CheckMessages(string mailboxId);

        Bundle GetMessage(string messageId);

        void RemoveMessage(string mailboxId, string messageId);
    }
}
