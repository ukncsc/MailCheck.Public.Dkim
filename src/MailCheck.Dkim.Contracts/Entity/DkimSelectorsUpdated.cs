using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.Entity
{
    public class DkimSelectorsUpdated : VersionedMessage
    {
        public DkimSelectorsUpdated(string id, int version, List<string> selectors) 
            : base(id, version)
        {
            Selectors = selectors;
        }

        public List<string> Selectors { get; }
    }
}