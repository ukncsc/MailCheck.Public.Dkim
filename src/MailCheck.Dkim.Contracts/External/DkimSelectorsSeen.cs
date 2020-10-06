using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.External
{
    public class DkimSelectorsSeen : Message
    {
        public DkimSelectorsSeen(string correlationId, string causationId,
            string id, List<string> selectors)
            : base(id)
        {
            CausationId = causationId;
            CorrelationId = correlationId;
            Selectors = selectors;
        }

        public List<string> Selectors { get; }
    }
}