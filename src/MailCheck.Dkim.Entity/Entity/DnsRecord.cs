using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity
{
    public class DnsRecord : IEquatable<DnsRecord>
    {
        public DnsRecord(string record, List<string> recordParts)
        {
            Record = record;
            RecordParts = recordParts;
        }

        public string Record { get; }

        public List<string> RecordParts { get; }

        public bool Equals(DnsRecord other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Record, other.Record) && RecordParts.CollectionEqual(other.RecordParts);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DnsRecord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Record != null ? Record.GetHashCode() : 0) * 397) ^
                       (RecordParts != null ? RecordParts.GetHashCode() : 0);
            }
        }
    }
}