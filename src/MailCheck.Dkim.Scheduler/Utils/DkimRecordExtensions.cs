using System;
using MailCheck.Dkim.Contracts.Scheduler;
using MailCheck.Dkim.Scheduler.Dao.Model;

namespace MailCheck.Dkim.Scheduler.Utils
{
    public static class DkimRecordExtensions
    {
        public static DkimRecordsExpired ToDkimPollMessage(this DkimSchedulerState dkimSchedulerRecord)
        {
            return new DkimRecordsExpired(dkimSchedulerRecord.Id, Guid.NewGuid().ToString(), null);
        }
    }
}
