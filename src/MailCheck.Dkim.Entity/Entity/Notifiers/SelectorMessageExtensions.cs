using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Entity.Entity.Notifications;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public static class SelectorMessageEnumerableExtensions
    {
        public static IEnumerable<SelectorMessages> GroupUpSelectorMessages(this IEnumerable<SelectorMessage> subject)
        {
            return subject
                .GroupBy(x => x.Selector, y => y.Message)
                .Select(x => new SelectorMessages(x.Key, x.Select(y => new AdvisoryMessage(y.Name, y.MessageType, y.Text)).ToList()));
        }
    }
}
