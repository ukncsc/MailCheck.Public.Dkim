using System.Collections.Generic;
using MailCheck.Dkim.Entity.Entity.Notifications;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public class SelectorRecordEqualityComparer : IEqualityComparer<SelectorRecord>
    {
        public bool Equals(SelectorRecord x, SelectorRecord y)
        {
            return x.Record.Equals(y.Record) && x.Selector.Equals(y.Selector);
        }

        public int GetHashCode(SelectorRecord obj)
        {
            return obj.GetHashCode();
        }
    }
}