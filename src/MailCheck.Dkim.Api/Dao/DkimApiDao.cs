using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dkim.Api.Domain;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Api.Dao
{
    public interface IDkimApiDao
    {
        Task<EntityDkimEntityState> GetDkimSelectors(string domain);
        Task<string> GetDkimHistory(string domain);
    }

    public class DkimApiApiDao : IDkimApiDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public DkimApiApiDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfo = connectionInfoAsync;
        }

        public async Task<EntityDkimEntityState> GetDkimSelectors(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            string serializedDkim = (string) await MySqlHelper.ExecuteScalarAsync(
                connectionString, DkimApiDaoResources.GetDkimResult, new MySqlParameter("domain", domain));

            return serializedDkim == null
                ? null
                : JsonConvert.DeserializeObject<EntityDkimEntityState>(serializedDkim, _serializerSettings);
        }

        public async Task<string> GetDkimHistory(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            string history = (string) await MySqlHelper.ExecuteScalarAsync(
                connectionString, DkimApiDaoResources.GetDkimHistory, new MySqlParameter("domain", domain));

            return history;
        }
    }
}