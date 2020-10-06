using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator.Domain;

namespace MailCheck.Dkim.Contracts.Evaluator
{
    public class DkimRecordEvaluationResult : Message
    {
        public DkimRecordEvaluationResult(string id, List<DkimSelectorResult> selectorResults)
            : base(id)
        {
            SelectorResults = selectorResults;
        }

        public List<DkimSelectorResult> SelectorResults { get; }
    }
}
