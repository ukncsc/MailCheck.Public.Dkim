using System;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.External
{
    public class DomainDeleted : Message
    {
        public DomainDeleted(string id)
            : base(id)
        {
            CausationId = null;
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}