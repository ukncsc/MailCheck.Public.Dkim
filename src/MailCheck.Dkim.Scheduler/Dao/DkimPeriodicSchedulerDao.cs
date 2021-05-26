using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MailCheck.Common.Util;
using MailCheck.Common.Data;
using Dapper;
using System;

namespace MailCheck.Dkim.Scheduler.Dao
{
    public interface IDkimPeriodicSchedulerDao
    {
        Task UpdateLastChecked(IEnumerable<DkimSchedulerState> entitiesToUpdate);
        Task<List<DkimSchedulerState>> GetDkimRecordsToUpdate();
    }
    
    public class DkimPeriodicSchedulerDao : IDkimPeriodicSchedulerDao
    {
        private readonly IDatabase _database;
        private readonly IClock _clock;
        private readonly IDkimSchedulerConfig _config;

        public DkimPeriodicSchedulerDao(IDatabase database,
            IDkimSchedulerConfig config, IClock clock)
        {
            _database = database;
            _config = config;
            _clock = clock;
        }

        public async Task UpdateLastChecked(IEnumerable<DkimSchedulerState> entitiesToUpdate)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var parameters = entitiesToUpdate.Select(ent => new { id = ent.Id, lastChecked = GetAdjustedLastCheckedTime() }).ToArray();
                await connection.ExecuteAsync(DkimPeriodicSchedulerDaoResources.UpdateDkimRecordsLastCheckedDistributed, parameters);
            }
        }

        public async Task<List<DkimSchedulerState>> GetDkimRecordsToUpdate()
        {
            DateTime nowMinusInterval = _clock.GetDateTimeUtc().AddSeconds(-_config.RefreshIntervalSeconds);
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var records = (await connection.QueryAsync<string>(DkimPeriodicSchedulerDaoResources.SelectDkimRecordsToSchedule,
                    new {now_minus_interval = nowMinusInterval, limit = _config.DomainsLimit})).ToList();

                return records.Select(record => new DkimSchedulerState(record)).ToList();
            }
        }

        private DateTime GetAdjustedLastCheckedTime()
        {
            return _clock.GetDateTimeUtc().AddSeconds(-(new Random().NextDouble() * 3600));
        }
    }
}