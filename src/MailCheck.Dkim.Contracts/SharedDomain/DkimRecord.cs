using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.SharedDomain
{
    public class DkimRecord
    {
        public DkimRecord(DnsRecord dnsRecord, List<Message> messages = null)
        {
            DnsRecord = dnsRecord;
            Messages = messages;
        }

        public DnsRecord DnsRecord { get; }

        public List<Message> Messages { get; }
    }
}