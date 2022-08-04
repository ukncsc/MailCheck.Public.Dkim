using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Api.Domain
{
    public class EntityDkimEntityState
    {
        public EntityDkimEntityState(string id,
            EntityDkimState state,
            int version,
            DateTime created,
            DateTime? recordsLastUpdated,
            DateTime? evaluationsLastUpdated,
            List<EntityDkimSelector> selectors)
        {
            Id = id;
            State = state;
            Version = version;
            Created = created;
            RecordsLastUpdated = recordsLastUpdated;
            EvaluationsLastUpdated = evaluationsLastUpdated;
            Selectors = selectors ?? new List<EntityDkimSelector>();
        }

        public virtual string Id { get; }
        public EntityDkimState State { get; }
        public virtual int Version { get; }
        public virtual DateTime Created { get; }
        public virtual DateTime? RecordsLastUpdated { get; }
        public virtual DateTime? EvaluationsLastUpdated { get; }
        public virtual List<EntityDkimSelector> Selectors { get; }
    }

    public class EntityDkimSelector
    {
        public EntityDkimSelector(string selector, List<EntityDkimRecord> records = null, string cName = null, EntityMessage pollError = null)
        {
            Selector = selector;
            Records = records;
            CName = cName;
            PollError = pollError;
        }

        public string Selector { get; }

        public List<EntityDkimRecord> Records { get; }

        public string CName { get; }

        public EntityMessage PollError { get; }
    }

    public class EntityDkimRecord
    {
        public EntityDkimRecord(EntityDnsRecord dnsRecord, List<EntityMessage> messages = null)
        {
            DnsRecord = dnsRecord;
            Messages = messages;
        }

        public EntityDnsRecord DnsRecord { get; }

        public List<EntityMessage> Messages { get; }
    }

     public class EntityDnsRecord
    {
        public EntityDnsRecord(string record, List<string> recordParts)
        {
            Record = record;
            RecordParts = recordParts;
        }

        public string Record { get; }

        public List<string> RecordParts { get; }
    }

    public class EntityMessage
    {
        public EntityMessage(string name, string text, string messageType, string markDown)
        {
            Name = name;
            Text = text;
            MessageType = messageType;
            MarkDown = markDown;
        }

        public string Name { get; }
        public string Text { get; }
        public string MessageType { get; }
        public string MarkDown { get; }
    }

    public enum EntityDkimState
    {
        Created,
        PollPending,
        Evaluated
    }
}
