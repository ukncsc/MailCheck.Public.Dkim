using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.Entity.Domain
{
    public class DkimEvaluationResults
    {
        public DkimEvaluationResults(string selector, List<DkimEvaluationRecord> records)
        {
            Selector = selector;
            Records = records;
        }

        public string Selector { get; }

        public List<DkimEvaluationRecord> Records { get; }
    }
}
