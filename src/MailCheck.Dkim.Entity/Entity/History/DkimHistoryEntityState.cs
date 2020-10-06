using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.Entity.Entity.History
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
    }
}