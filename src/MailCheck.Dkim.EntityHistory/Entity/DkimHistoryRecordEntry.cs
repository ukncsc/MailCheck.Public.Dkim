using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.EntityHistory.Entity
{
    public class DkimHistoryRecordEntry : IEquatable<DkimHistoryRecordEntry>
    {
        public DkimHistoryRecordEntry(string selector, List<string> dnsRecords)
        {
            Selector = selector;
            DnsRecords = dnsRecords ?? new List<string>();
        }

        public string Selector { get; }
        public List<string> DnsRecords { get; }

        public bool Equals(DkimHistoryRecordEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Selector, other.Selector) &&
                   DnsRecords.Count == other.DnsRecords.Count &&
                   DnsRecords.All(other.DnsRecords.Contains);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DkimHistoryRecordEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Selector != null ? Selector.GetHashCode() : 0) * 397) ^ (DnsRecords != null ? DnsRecords.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DkimHistoryRecordEntry left, DkimHistoryRecordEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DkimHistoryRecordEntry left, DkimHistoryRecordEntry right)
        {
            return !Equals(left, right);
        }
    }
}