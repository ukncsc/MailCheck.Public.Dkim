using System.Collections.Generic;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class DkimSelectorRecords
    {
        public DkimSelectorRecords(DkimSelector selector, List<DkimRecord> records)
        {
            Selector = selector;
            Records = records;
        }

        public DkimSelector Selector { get; }
        public List<DkimRecord> Records { get; }
    }
}