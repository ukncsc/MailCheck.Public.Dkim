using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.Entity.Domain
{
    public class DkimEvaluationRecord
    {
        public DkimEvaluationRecord(string record, List<DkimEvaluationMessage> evaluationMessages)
        {
            Record = record;
            EvaluationMessages = evaluationMessages;
        }

        public string Record { get; }

        public List<DkimEvaluationMessage> EvaluationMessages { get; }
    }
}
