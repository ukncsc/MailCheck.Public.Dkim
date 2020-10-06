using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.SharedDomain
{
    public class DkimSelector
    {
        public DkimSelector(string selector, List<DkimRecord> records = null, string cName = null, Message pollError = null)
        {
            Selector = selector;
            Records = records ?? new List<DkimRecord>();
            CName = cName;
            PollError = pollError;
        }

        public string Selector { get; }
        public List<DkimRecord> Records { get; }
        public string CName { get; }
        public Message PollError { get; }
    }
}
