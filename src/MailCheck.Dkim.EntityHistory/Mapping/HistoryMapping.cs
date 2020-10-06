using System.Linq;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.EntityHistory.Entity;

namespace MailCheck.Dkim.EntityHistory.Mapping
{
    public static class HistoryMapping
    {
        public static DkimHistoryRecord ToDkimHistoryRecord(this DkimEvaluationUpdated dkimevaluationUpdated)
        {
            return new DkimHistoryRecord(dkimevaluationUpdated.Timestamp, null,
                dkimevaluationUpdated.DkimEvaluationResults?.Where(_ => _.Records != null && _.Records.Any())
                    .Select(_ => new DkimHistoryRecordEntry(_.Selector, _.Records.Select(r => r.Record).ToList()))
                    .ToList());
        }
    }
}