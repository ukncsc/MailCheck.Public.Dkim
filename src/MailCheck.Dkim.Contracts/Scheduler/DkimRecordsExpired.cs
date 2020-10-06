using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.Scheduler
{
    public class DkimRecordsExpired : Message
    {
        public DkimRecordsExpired(string id, string correlationId, string causationId) 
            : base(id)
        {
            CorrelationId = correlationId;
            CausationId = causationId;
        }
    }
}
