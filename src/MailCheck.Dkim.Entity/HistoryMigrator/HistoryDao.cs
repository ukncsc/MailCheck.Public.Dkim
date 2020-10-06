using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.History;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Entity.HistoryMigrator
{
    public interface IHistoryDao
    {
        Task<List<string>> GetDomains();
        Task<List<DkimEntityState>> GetHistoryForDomains(List<string> domains);
        Task AddHistoryForDomain(List<DkimHistoryEntityState> states);

        Task<List<string>> GetDomainsWithoutHistory();
        Task<List<DkimEntityState>> GetEntityForDomains(List<string> domains);
    }

    public class HistoryDao : IHistoryDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public HistoryDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<string>> GetDomains()
        {
            List<string> domains = new List<string>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                "SELECT distinct(entity_id) as domain FROM dkim_entity_history_old"))
            {
                while (await reader.ReadAsync())
                {
                    domains.Add(reader.GetString("domain"));
                }
            }

            return domains;
        }

        public async Task<List<string>> GetDomainsWithoutHistory()
        {
            List<string> domains = new List<string>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                "SELECT e.id FROM dkim_entity e LEFT JOIN dkim_entity_history eh ON e.id = eh.id WHERE eh.id IS NULL"))
            {
                while (await reader.ReadAsync())
                {
                    domains.Add(reader.GetString("id"));
                }
            }

            return domains;
        }

        public async Task<List<DkimEntityState>> GetEntityForDomains(List<string> domains)
        {
            string domainsString = string.Join(",", domains.Select((v, i) => $"@domain{i}"));

            string selectHistoryForDomains = $"SELECT state FROM dkim_entity where id in ({domainsString});";

            MySqlParameter[] parameters = domains.Select((v, i) => new MySqlParameter($"domain{i}", v)).ToArray();

            List<DkimEntityState> states = new List<DkimEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                selectHistoryForDomains, parameters))
            {
                while (await reader.ReadAsync())
                {
                    states.Add(JsonConvert.DeserializeObject<DkimEntityState>(reader.GetString("state")));
                }
            }

            return states;
        }

        public async Task<List<DkimEntityState>> GetHistoryForDomains(List<string> domains)
        {
            string domainsString = string.Join(",",domains.Select((v, i) => $"@domain{i}"));

            string selectHistoryForDomains = $"SELECT state FROM dkim_entity_history_old where entity_id in ({domainsString});";

            MySqlParameter[] parameters = domains.Select((v, i) => new MySqlParameter($"domain{i}", v)).ToArray();

            List<DkimEntityState> states = new List<DkimEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                selectHistoryForDomains, parameters))
            {
                while (await reader.ReadAsync())
                {
                    states.Add(JsonConvert.DeserializeObject<DkimEntityState>(reader.GetString("state")));
                }
            }

            return states;
        }

        public async Task AddHistoryForDomain(List<DkimHistoryEntityState> states)
        {
            string individualInsertStatement = string.Join(",", states.Select((v, i) => $"(@id{i},@state{i})"));

            string insert = $"INSERT INTO `dkim_entity_history`(`id`,`state`) VALUES {individualInsertStatement};";

            MySqlParameter[] parameters = states.Select((v, i) => new MySqlParameter($"id{i}", v.Id))
                .Concat(states.Select((v, i) =>
               {
                   JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                   {
                       ContractResolver = new CamelCasePropertyNamesContractResolver()
                   };

                   return new MySqlParameter($"state{i}", JsonConvert.SerializeObject(v, jsonSerializerSettings));
               })).ToArray();

            await MySqlHelper.ExecuteNonQueryAsync(_connectionInfo.ConnectionString, insert, parameters);
        }
    }
}