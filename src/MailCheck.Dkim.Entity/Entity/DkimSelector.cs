using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.Entity.Entity
{
    public class DkimSelector
    {
        public DkimSelector(string selector, List<DkimRecord> records = null, string cName = null, Message pollError = null)
        {
            Selector = selector;
            CName = cName;
            Records = records ?? new List<DkimRecord>();
            PollError = pollError;
        }

        public string Selector { get; }
        public string CName { get; private set; }
        public List<DkimRecord> Records { get; private set; }
        public Message PollError { get; private set; }

        public void UpdateCName(string cName)
        {
            CName = cName;
        }

        public bool UpdatePollError(Message pollError)
        {
            bool updated = false;

            if (!pollError.Equals(PollError))
            {
                PollError = pollError;
                Records.Clear();
                updated = true;
            }

            return updated;
        }

        public bool UpdateRecords(List<DkimRecord> records)
        {
            bool updated = false;

            Dictionary<string, DkimRecord> recordLookUp = Records?.ToDictionary(_ => _.DnsRecord.Record) ?? new Dictionary<string, DkimRecord>();
            List<DkimRecord> newRecords = new List<DkimRecord>();
            foreach (DkimRecord record in records)
            {
                DkimRecord newRecord;
                if (recordLookUp.TryGetValue(record.DnsRecord.Record, out DkimRecord dkimRecordEvaluation) &&
                    dkimRecordEvaluation.DnsRecord.Equals(record.DnsRecord))
                {
                    newRecord = dkimRecordEvaluation;
                }
                else
                {
                    newRecord = record;
                    updated = true;
                }
                newRecords.Add(newRecord);
            }

            Records = newRecords;
            PollError = null;
            
            return updated;
        }

        public bool UpdateEvaluations(List<DkimRecord> records)
        {
            bool updated = false;
            Dictionary<string, DkimRecord> recordLookUp = records.ToDictionary(_ => _.DnsRecord.Record);
            List<DkimRecord> newRecords = new List<DkimRecord>();

            foreach (DkimRecord dkimRecord in Records)
            {
                if (recordLookUp.TryGetValue(dkimRecord.DnsRecord.Record, out DkimRecord dkimRecordEvaluation) &&
                    !dkimRecordEvaluation.EvaluationMessages.CollectionEqual(dkimRecord.EvaluationMessages))
                {
                    newRecords.Add(new DkimRecord(dkimRecord.DnsRecord, dkimRecordEvaluation.EvaluationMessages));
                    updated = true;
                }
                else
                {
                    if (dkimRecord.EvaluationMessages == null)
                    {
                        newRecords.Add(new DkimRecord(dkimRecord.DnsRecord, new List<Message>()));
                        updated = true;
                    }
                    else
                    {
                        newRecords.Add(dkimRecord);
                    }
                }
            }

            Records = newRecords;
            return updated;
        }
    }
}