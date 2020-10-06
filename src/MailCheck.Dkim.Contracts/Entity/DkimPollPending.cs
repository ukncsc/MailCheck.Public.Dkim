using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.Entity
{
    public class DkimPollPending : VersionedMessage
    {
        public DkimPollPending(string id, int version, List<string> selectors) 
            : base(id, version)
        {
            Selectors = selectors;
        }

        public List<string> Selectors { get; }

        public DkimState State => DkimState.PollPending;
    }
}