using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Scheduler.Dao
{
    public interface IDkimPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<DkimSchedulerState> entitiesToUpdate);
        Task<List<DkimSchedulerState>> GetDkimRecordsToUpdate();
    }

    public class DkimPeriodicSchedulerDao : IDkimPeriodicSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly IDkimSchedulerConfig _config;

        public DkimPeriodicSchedulerDao(IConnectionInfoAsync connectionInfo,
            IDkimSchedulerConfig config)
        {
            _connectionInfo = connectionInfo;
            _config = config;
        }

        public async Task UpdateLastChecked(List<DkimSchedulerState> entitiesToUpdate)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            string query = string.Format(DkimPeriodicSchedulerDaoResources.UpdateDkimRecordsLastChecked,
                string.Join(',', entitiesToUpdate.Select((_, i) => $"@domainName{i}")));

            MySqlParameter[] parameters =
                entitiesToUpdate.Select((v, i) => new MySqlParameter($"domainName{i}", v.Id.ToLower())).ToArray();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                query,
                parameters);
        }

        public async Task<List<DkimSchedulerState>> GetDkimRecordsToUpdate()
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            MySqlParameter[] parameters =
            {
                new MySqlParameter("refreshIntervalSeconds", _config.RefreshIntervalSeconds),
                new MySqlParameter("limit", _config.DomainsLimit),
            };

            List<DkimSchedulerState> dkimStates = new List<DkimSchedulerState>();
            using (var reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                DkimPeriodicSchedulerDaoResources.SelectDkimRecordsToSchedule, parameters))
            {
                while (await reader.ReadAsync())
                {
                    dkimStates.Add(new DkimSchedulerState(reader.GetString("id")));
                }
            }

            return dkimStates;
        }
    }
}