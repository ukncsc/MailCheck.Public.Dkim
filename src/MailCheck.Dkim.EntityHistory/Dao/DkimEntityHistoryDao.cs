using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dkim.EntityHistory.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.EntityHistory.Dao
{
    public interface IDkimHistoryEntityDao
    {
        Task<DkimHistoryEntityState> Get(string domain);
        Task Save(DkimHistoryEntityState state);
    }

    public class DkimHistoryEntityDao : IDkimHistoryEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public DkimHistoryEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<DkimHistoryEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, DkimEntityHistoryDaoResouces.SelectDkimEntityHistory,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<DkimHistoryEntityState>(state);
        }

        public async Task Save(DkimHistoryEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DkimEntityHistoryDaoResouces.InsertDkimEntityHistory,
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", serializedState));
        }
    }
}
