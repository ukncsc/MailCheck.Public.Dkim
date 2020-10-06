using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Scheduler.Dao
{
    public interface IDkimSchedulerDao
    {
        Task<DkimSchedulerState> Get(string domain);
        Task Save(DkimSchedulerState schedulerState);
        Task Delete(string domain);
    }

    public class DkimSchedulerDao : IDkimSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;

        public DkimSchedulerDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<DkimSchedulerState> Get(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            string id = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, DkimSchedulerDaoResources.SelectDkimRecord,
                new MySqlParameter("domain", domain));

            return id == null
                ? null
                : new DkimSchedulerState(id);
        }

        public async Task Save(DkimSchedulerState state)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            int numberOfRowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DkimSchedulerDaoResources.InsertOrUpdateRecord,
                new MySqlParameter("domain", state.Id.ToLower()));

            if (numberOfRowsAffected == 0)
            {
                throw new InvalidOperationException($"Didn't save duplicate {nameof(DkimSchedulerState)} for {state.Id}");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DkimSchedulerDaoResources.DeleteDkimRecord,
                new MySqlParameter("id", domain));
        }
    }
}
