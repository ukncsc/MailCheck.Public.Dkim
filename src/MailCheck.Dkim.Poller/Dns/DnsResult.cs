using System;
using MailCheck.Dkim.Contracts.Poller;

namespace MailCheck.Dkim.Poller.Dns
{
    public class DnsResult<T>
        where T : class
    {
        public DnsResult(T value)
            : this(value, null, null, null)
        {
        }

        public DnsResult(T value,  string nameServer, string auditTrail)
            : this(value, null, nameServer, auditTrail)
        {
        }
        public DnsResult(string error)
            : this(error, null, null) { }

        public DnsResult(string error, string nameServer, string auditTrail)
            : this(null, error, nameServer, auditTrail) { }

        private DnsResult(T value, string error, string nameServer, string auditTrail)
        {
            Error = error;
            NameServer = nameServer;
            AuditTrail = auditTrail;
            Value = value;
        }

        public string NameServer { get; }

        public string AuditTrail { get; }
        
        public string Error { get; }

        public bool IsErrored => Error != null;

        public T Value { get; }
    }
}
