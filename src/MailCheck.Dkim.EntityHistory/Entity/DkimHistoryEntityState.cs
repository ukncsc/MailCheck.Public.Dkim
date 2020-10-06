using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.EntityHistory.Entity
{
    public class DkimHistoryEntityState
    {
        public DkimHistoryEntityState(string id, List<DkimHistoryRecord> records = null)
        {
            Id = id;
            Records = records ?? new List<DkimHistoryRecord>();
        }

        public string Id { get; }

        public List<DkimHistoryRecord> Records { get; }

        public bool UpdateHistory(DkimHistoryRecord record)
        {
            bool updated = false;

            DkimHistoryRecord currentRecord = Records.FirstOrDefault();

            if (currentRecord == null)
            {
                Records.Add(record);
                updated = true;
            }
            else if (!currentRecord.Entries.CollectionEqual(record.Entries, new DkimEntityHistoryComparer()))
            {
                currentRecord.EndDate = record.StartDate;
                Records.Insert(0, record);
                updated = true;
            }


            return updated;
        }
    }
}
