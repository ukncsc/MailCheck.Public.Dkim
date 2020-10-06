using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.SharedDomain
{
    public class DnsRecord 
    {
        public DnsRecord(string record, List<string> recordParts)
        {
            Record = record;
            RecordParts = recordParts;
        }

        public string Record { get; }

        public List<string> RecordParts { get; }
    }
}