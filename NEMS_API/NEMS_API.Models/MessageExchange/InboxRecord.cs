namespace NEMS_API.Models.MessageExchange
{
    public class InboxRecord
    {
        public string MessageID { get; set; }

        public string ErrorEvent { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorDescription { get; set; }
    }
}