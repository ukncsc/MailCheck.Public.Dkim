using System.Collections.Generic;

namespace MailCheck.Dkim.EntityHistory.Entity
{
    internal class DkimEntityHistoryComparer : IEqualityComparer<DkimHistoryRecordEntry>
    {
        public bool Equals(DkimHistoryRecordEntry x, DkimHistoryRecordEntry y)
        {
            return x.Selector == y.Selector && x.DnsRecords.CollectionEqual(y.DnsRecords) && x.DnsRecords.Count == y.DnsRecords.Count;
        }

        public int GetHashCode(DkimHistoryRecordEntry obj)
        {
            return obj.GetHashCode();
        }
    }
}