using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class DkimRecordAdded : Common.Messaging.Abstractions.Message
    {
        public DkimRecordAdded(string id, List<SelectorRecords> selectorRecords) : base(id)
        {
            SelectorRecords = selectorRecords;
        }

        public List<SelectorRecords> SelectorRecords { get; }
    }
}