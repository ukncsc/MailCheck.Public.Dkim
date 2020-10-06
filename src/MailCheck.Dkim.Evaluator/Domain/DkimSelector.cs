using System.Collections.Generic;
using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class DkimSelector
    {
        public DkimSelector(string selector, string cName, List<DkimRecord> records = null, Message pollError = null)
        {
            Selector = selector;
            CName = cName;
            Records = records ?? new List<DkimRecord>();
            PollError = pollError; 
        }

        public string Selector { get; }
        public string CName { get; }
        public List<DkimRecord> Records { get; }
        public Message PollError { get; }
    }
}
