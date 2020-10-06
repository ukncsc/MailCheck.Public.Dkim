using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.EntityHistory.Entity
{
    public class DkimHistoryRecord
    {
        public DkimHistoryRecord(DateTime startDate, DateTime? endDate, List<DkimHistoryRecordEntry> entries)
        {
            StartDate = startDate;
            EndDate = endDate;
            Entries = entries ?? new List<DkimHistoryRecordEntry>();
        }

        public DateTime StartDate { get; }
        public DateTime? EndDate { get; set; }
        public List<DkimHistoryRecordEntry> Entries { get; }
    }
}