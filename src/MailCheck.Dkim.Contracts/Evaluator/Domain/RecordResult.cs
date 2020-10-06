using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.Evaluator.Domain
{
    public class RecordResult
    {
        public RecordResult(string record, List<DkimEvaluatorMessage> messages)
        {
            Messages = messages;
            Record = record;
        }

        public string Record { get; }
        public List<DkimEvaluatorMessage> Messages { get; }
    }
}