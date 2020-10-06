using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dkim.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Entity.Dao
{
    public interface IDkimEntityDao
    {
        Task<DkimEntityState> Get(string domain);
        Task Save(DkimEntityState state);
        Task Delete(string domain);
    }

    public class DkimEntityDao : IDkimEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public DkimEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<DkimEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string) await MySqlHelper.ExecuteScalarAsync(connectionString,
                DkimEntityDaoResources.SelectDkimEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<DkimEntityState>(state);
        }

        public async Task Save(DkimEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DkimEntityDaoResources.InsertDkimEntity,
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", serializedState));

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Didn't update DkimEntityState because version {state.Version} has already been persisted.");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                DkimEntityDaoResources.DeleteDkimEntity,
                new MySqlParameter("domain", domain));
        }
    }
}