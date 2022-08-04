using System;
using System.Collections.Generic;
using MailCheck.Dkim.Entity.Entity.Notifications;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public class SelectorMessageEqualityComparer : IEqualityComparer<SelectorMessage>
    {
        public bool Equals(SelectorMessage x, SelectorMessage y)
        {
            return x.Message.Name.Equals(y.Message.Name) && x.Selector.Equals(y.Selector);
        }

        public int GetHashCode(SelectorMessage obj)
        {
            return obj.GetHashCode();
        }
    }
}