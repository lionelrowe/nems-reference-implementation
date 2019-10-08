using Hl7.Fhir.Model;
using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Models.MessageExchange;
using System.Linq;

namespace NEMS_API.Services
{
    public class MessageExchangeService : IMessageExchangeService
    {

        private readonly IDataReader _dataReader;
        private readonly IDataWriter _dataWriter;

        public MessageExchangeService(IDataReader dataReader, IDataWriter dataWriter)
        {
            _dataReader = dataReader;
            _dataWriter = dataWriter;
        }

        public InboxRecord CheckMessages(string mailboxId)
        {
            var identifier = new InboxRecordIdentifier
            {
                Data = mailboxId
            };

            var mailboxEntries = _dataReader.Search(identifier);

            if (mailboxEntries.Count < 1)
            {
                return null;
            }

            var entry = new InboxRecord
            {
                MessageID = mailboxEntries.First().Id
            };

            return entry;
        }

        public Bundle GetMessage(string messageId)
        {
            var mailboxEntry = _dataReader.Read<Bundle>(CacheKeys.NemsEventEntry(messageId));

            return mailboxEntry;
        }

        public void RemoveMessage(string mailboxId, string messageId)
        {
            var entry = new InboxRecordIdentifier
            {
                Id = messageId,
                Data = mailboxId
            };

            _dataWriter.Delete(entry);
        }
    }
}
