using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Api.Domain
{
    public class DkimResponse
    {
        public DkimResponse(string domain, State state, List<DkimSelector> selectors, DateTime? recordsLastUpdated)
        {
            Domain = domain;
            State = state;
            Selectors = selectors;
            LastUpdated = recordsLastUpdated;
        }

        public string Domain { get; }
        public State State { get; }
        public List<DkimSelector> Selectors { get; }
        public DateTime? LastUpdated { get; }
    }

    public class DkimSelector
    {
        public DkimSelector(string selector, string cName, List<DkimRecord> records, List<DkimMessage> messages)
        {
            Selector = selector;
            CName = cName;
            Records = records ?? new List<DkimRecord>();
            Messages = messages ?? new List<DkimMessage>();
        }

        public string Selector { get; }
        
        public string CName { get; }

        public List<DkimRecord> Records { get; }

        public List<DkimMessage> Messages { get; }
    }

    public class DkimRecord
    {
        public DkimRecord(string record)
        {
            Record = record;
        }

        public string Record { get; }
    }

    public class DkimMessage
    {
        public DkimMessage(string severity, string message, string markDown)
        {
            Severity = severity;
            Message = message;
            MarkDown = markDown;
        }

        public string Severity { get; }
        public string Message { get; }
        public string MarkDown { get; }
    }

    public enum State
    {
        Created,
        PollPending,
        Evaluated
    }
}
