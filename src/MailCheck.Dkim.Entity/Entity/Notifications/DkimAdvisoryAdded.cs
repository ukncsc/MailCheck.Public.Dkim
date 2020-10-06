using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class DkimAdvisoryAdded : Common.Messaging.Abstractions.Message
    {
        public DkimAdvisoryAdded(string id, List<SelectorMessages> selectorMessages) : base(id)
        {
            SelectorMessages = selectorMessages;
        }

        public List<SelectorMessages> SelectorMessages { get; }
    }
}