using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Dkim.Migration;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;
using NUnit.Framework;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dkim.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DkimSchedulerDaoTests : DatabaseTestBase
    {
        private IDkimSchedulerDao _dkimSchedulerDao;
        private IDkimSchedulerConfig _config;
        private IDatabase _database;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            _config = A.Fake<IDkimSchedulerConfig>();
            _database = A.Fake<IDatabase>();

            A.CallTo(() => _config.RefreshIntervalSeconds).Returns(86400);
            A.CallTo(() => _config.DomainsLimit).Returns(10);

            _dkimSchedulerDao =
                new DkimSchedulerDao(_database);
        }

        [Test]
        public async Task InsertRecordTest()
        {
            DkimSchedulerState record = new DkimSchedulerState("abc.com");

            await _dkimSchedulerDao.Save(record);
            List<DkimSchedulerState> results = await GetValues();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().Id, Is.EqualTo(record.Id));
        }

        [Test]
        public async Task WhenUpdatingRecordWithOldVersionShouldThrowException()
        {
            DkimSchedulerState oldRecord = new DkimSchedulerState("abc.com");

            await _dkimSchedulerDao.Save(oldRecord);

            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(() => _dkimSchedulerDao.Save(oldRecord));
            Assert.That(exception.Message, Is.EqualTo($"Didn't save duplicate DkimSchedulerState for abc.com"));
        }

        protected override string GetDatabaseName()
        {
            return "dkimscheduler";
        }

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task<List<DkimSchedulerState>> GetValues()
        {
            List<DkimSchedulerState> results = new List<DkimSchedulerState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString, "SELECT id FROM dkim_scheduled_records"))
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new DkimSchedulerState(reader.GetString("id")));
                }
            }

            return results;
        }

        #endregion TestSupport
    }
}
