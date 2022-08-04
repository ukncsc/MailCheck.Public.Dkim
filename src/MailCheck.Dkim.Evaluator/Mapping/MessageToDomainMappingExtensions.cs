using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Contracts.SharedDomain;
using DkimRecord = MailCheck.Dkim.Evaluator.Domain.DkimRecord;
using DkimSelector = MailCheck.Dkim.Evaluator.Domain.DkimSelector;
using DkimSelectorRecords = MailCheck.Dkim.Contracts.Poller.DkimSelectorRecords;

namespace MailCheck.Dkim.Evaluator.Mapping
{
    public static class MessageToDomainMappingExtensions
    {
        public static List<DkimSelector> ToDkimRecords(this DkimRecordsPolled records)
        {
            return records.DkimSelectorRecords.Select(_ => _.ToDkimSelector()).ToList();
        }

        private static DkimSelector ToDkimSelector(this DkimSelectorRecords records)
        {
            return new DkimSelector(records.Selector, records.CName,
                records.Records?.Select(_ => new DkimRecord(new DnsRecord(_.Record, _.RecordParts))).ToList(),
                records.Error == null ? null : new Message(records.Id, records.ErrorName, MessageType.error, records.Error, records.ErrorMarkDown));
        }
    }
}
