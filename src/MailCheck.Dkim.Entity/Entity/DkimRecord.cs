using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity
{
    public class DkimRecord
    {
        public DkimRecord(DnsRecord dnsRecord, List<Message> evaluationMessages = null)
        {
            DnsRecord = dnsRecord;
            EvaluationMessages = evaluationMessages;
        }

        public DnsRecord DnsRecord { get; }

        public List<Message> EvaluationMessages { get; }
    }
}