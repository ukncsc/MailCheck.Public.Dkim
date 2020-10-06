using System.Collections.Generic;
using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Contracts.Evaluator.Domain
{
    public class DkimSelectorResult
    {
        public DkimSelectorResult(DkimSelector selector, List<RecordResult> recordsResults)
        {
            Selector = selector;
            RecordsResults = recordsResults;
        }

        public DkimSelector Selector { get; }
        public List<RecordResult> RecordsResults { get; }
    }
}