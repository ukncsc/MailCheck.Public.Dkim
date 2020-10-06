﻿using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;

namespace MailCheck.Dkim.Entity.Seeding
{
    internal interface IDomainDao
    {
        Task<List<Domain>> GetDomains();
    }

    internal class DomainDao : IDomainDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public DomainDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<Domain>> GetDomains()
        {
            List<Domain> domains = new List<Domain>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString,
                "SELECT d.name, d.created_date, u.email as created_by FROM domain d LEFT JOIN user u on u.id = d.created_by WHERE d.publish OR d.monitor;"))
            {
                while (await reader.ReadAsync())
                {
                    domains.Add(new Domain(
                        reader.GetString("name"),
                        reader.GetString("created_by"),
                        reader.GetDateTime("created_date")));
                }
            }

            return domains;
        }
    }
}