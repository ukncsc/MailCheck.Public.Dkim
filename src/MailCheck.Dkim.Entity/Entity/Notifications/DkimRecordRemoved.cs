using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class DkimRecordRemoved : Common.Messaging.Abstractions.Message
    {
        public DkimRecordRemoved(string id, List<SelectorRecords> selectorRecords) : base(id)
        {
            SelectorRecords = selectorRecords;
        }

        public List<SelectorRecords> SelectorRecords { get; }
    }
}