using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class DkimAdvisorySustained : Common.Messaging.Abstractions.Message
    {
        public DkimAdvisorySustained(string id, List<SelectorMessages> selectorMessages) : base(id)
        {
            SelectorMessages = selectorMessages;
        }

        public List<SelectorMessages> SelectorMessages { get; }
    }
}
