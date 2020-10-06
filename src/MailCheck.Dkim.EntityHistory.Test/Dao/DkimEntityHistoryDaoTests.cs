using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.Dkim.EntityHistory.Dao;
using MailCheck.Dkim.EntityHistory.Entity;
using MailCheck.Dkim.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.EntityHistory.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DkimEntityHistoryDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private IDkimHistoryEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new DkimHistoryEntityDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            DkimHistoryEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            string selector = "dkimRecord";

            DkimHistoryEntityState state = new DkimHistoryEntityState(Id,
                new List<DkimHistoryRecord>
                {
                    new DkimHistoryRecord(DateTime.UtcNow.AddDays(-1), null,
                        new List<DkimHistoryRecordEntry> {new DkimHistoryRecordEntry(selector, new List<string>())})
                });

            await Insert(state);

            DkimHistoryEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.Records.Count, Is.EqualTo(state.Records.Count));
            Assert.That(stateFromDatabase.Records[0].Entries.Count, Is.EqualTo(state.Records[0].Entries.Count));
            Assert.That(stateFromDatabase.Records[0].Entries[0], Is.EqualTo(state.Records[0].Entries[0]));
        }

        [Test]
        public async Task HistoryIsSavedForChanges()
        {
            string selector = "dkimRecord";
            string selector2 = "dkimRecord2";

            DkimHistoryEntityState state = new DkimHistoryEntityState(Id,
                new List<DkimHistoryRecord>
                {
                    new DkimHistoryRecord(DateTime.UtcNow.AddDays(-1), null,
                        new List<DkimHistoryRecordEntry>
                        {
                            new DkimHistoryRecordEntry(selector, new List<string>()),
                        })
                });

            await _dao.Save(state);

            DkimHistoryEntityState state2 = (await SelectAllHistory(Id)).First();
            state2.Records[0].EndDate = DateTime.UtcNow;
            state2.Records.Insert(0, new DkimHistoryRecord(DateTime.UtcNow.AddDays(-1), null,
                new List<DkimHistoryRecordEntry>
                {
                    new DkimHistoryRecordEntry(selector2, new List<string>()),
                }));

            await _dao.Save(state2);

            List<DkimHistoryEntityState> historyStates = await SelectAllHistory(Id);
            Assert.That(historyStates[0].Records.Count, Is.EqualTo(2));

            Assert.That(historyStates[0].Records[0].EndDate, Is.Null);
            Assert.That(historyStates[0].Records[0].Entries.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].Records[0].Entries[0], Is.EqualTo(selector2));

            Assert.That(historyStates[0].Records[1].EndDate, Is.Not.Null);
            Assert.That(historyStates[0].Records[1].Entries.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].Records[1].Entries[0], Is.EqualTo(selector));
        }

        protected override string GetDatabaseName() => "dkimHistoryEntity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(DkimHistoryEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `dkim_entity_history`(`id`,`state`)VALUES(@domain,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<DkimHistoryEntityState>> SelectAllHistory(string id)
        {
            List<DkimHistoryEntityState> list = new List<DkimHistoryEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM dkim_entity_history WHERE id = @domain ORDER BY id;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<DkimHistoryEntityState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}