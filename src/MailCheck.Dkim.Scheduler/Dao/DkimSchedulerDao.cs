using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
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
        Task<int> Delete(string domain);
    }

    public class DkimSchedulerDao : IDkimSchedulerDao
    {
        private readonly IDatabase _database;
        

        public DkimSchedulerDao(IDatabase database)
        {
            _database = database;
        }

        public async Task<DkimSchedulerState> Get(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                string id = await connection.QueryFirstOrDefaultAsync<string>(
                    DkimSchedulerDaoResources.SelectDkimRecord, new {domain});
                
                return id == null
                    ? null
                    : new DkimSchedulerState(id);
            }
        }

        public async Task Save(DkimSchedulerState state)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                int numberOfRowsAffected = await connection.ExecuteAsync(DkimSchedulerDaoResources.InsertOrUpdateRecord,
                    new {domain = state.Id.ToLower()});
                
                if (numberOfRowsAffected == 0)
                {
                    throw new InvalidOperationException($"Didn't save duplicate {nameof(DkimSchedulerState)} for {state.Id}");
                }
            }
        }

        public async Task<int> Delete(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                return await connection.ExecuteAsync(DkimSchedulerDaoResources.DeleteDkimRecord,
                    new { id = domain });
            }
        }
    }
}
