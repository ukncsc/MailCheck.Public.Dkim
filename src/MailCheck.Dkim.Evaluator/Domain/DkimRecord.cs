using System.Collections.Generic;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class DkimRecord
    {
        public DkimRecord(DnsRecord dnsRecord, List<Tag> tags = null, List<EvaluationError> errors = null)
        {
            DnsRecord = dnsRecord;
            Tags = tags ?? new List<Tag>();
            Errors = errors ?? new List<EvaluationError>();
        }

        public DnsRecord DnsRecord { get; }

        public List<Tag> Tags { get; }

        public List<EvaluationError> Errors { get; }
    }
}
