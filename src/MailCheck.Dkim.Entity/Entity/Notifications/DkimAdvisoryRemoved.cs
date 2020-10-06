using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class DkimAdvisoryRemoved : Common.Messaging.Abstractions.Message
    {
        public DkimAdvisoryRemoved(string id, List<SelectorMessages> selectorMessages) : base(id)
        {
            SelectorMessages = selectorMessages;
        }

        public List<SelectorMessages> SelectorMessages { get; }
    }
}