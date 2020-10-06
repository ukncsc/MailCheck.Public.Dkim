using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.Poller
{
    public class DkimRecordsPolled : Message
    {
        public DkimRecordsPolled(string id, List<DkimSelectorRecords> dkimSelectorRecords)
            : base(id)
        {
            DkimSelectorRecords = dkimSelectorRecords;
        }

        public List<DkimSelectorRecords> DkimSelectorRecords { get; }
    }
}