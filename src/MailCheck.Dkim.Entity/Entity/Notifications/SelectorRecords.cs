using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class SelectorRecords
    {
        public string Selector { get; }
        public List<string> Records { get; }

        public SelectorRecords(string selector, List<string> records)
        {
            Selector = selector;
            Records = records;
        }
    }
}