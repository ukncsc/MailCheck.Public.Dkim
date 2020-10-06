using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class SelectorMessages
    {
        public string Selector { get; }
        public List<AdvisoryMessage> Messages { get; }

        public SelectorMessages(string selector, List<AdvisoryMessage> messages)
        {
            Selector = selector;
            Messages = messages;
        }
    }
}