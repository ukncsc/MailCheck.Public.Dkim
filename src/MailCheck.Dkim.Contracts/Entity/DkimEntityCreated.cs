using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Dkim.Contracts.Entity
{
    public class DkimEntityCreated : VersionedMessage
    {
        public DkimEntityCreated(string id, int version) 
            : base(id, version)
        {
        }

        public DkimState State => DkimState.Created;
    }
}