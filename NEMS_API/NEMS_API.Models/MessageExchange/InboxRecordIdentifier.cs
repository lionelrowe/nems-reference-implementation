using NEMS_API.Models.Core;

namespace NEMS_API.Models.MessageExchange
{
    public class InboxRecordIdentifier : DataItem
    {
        public override string CacheKey
        {
            get
            {
                return CacheKeys.Mailbox(Data);
            }
        }
    }
}
