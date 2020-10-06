using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.Dkim.Migration;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;
using NUnit.Framework;

namespace MailCheck.Dkim.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DkimPeriodicSchedulerDaoTests : DatabaseTestBase
    {
        private IConnectionInfoAsync _connectionInfoAsync;
        private IDkimSchedulerConfig _config;
        private DkimPeriodicSchedulerDao _dkimPeriodicSchedulerDao;
        private DkimSchedulerDao _dkimSchedulerDao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            _connectionInfoAsync = A.Fake<IConnectionInfoAsync>();

            _config = A.Fake<IDkimSchedulerConfig>();

            A.CallTo(() => _config.RefreshIntervalSeconds).Returns(86400);
            A.CallTo(() => _config.DomainsLimit).Returns(10);
            A.CallTo(() => _connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dkimPeriodicSchedulerDao = new DkimPeriodicSchedulerDao(
                _connectionInfoAsync, 
                _config);

            _dkimSchedulerDao = new DkimSchedulerDao(_connectionInfoAsync);
        }

        [Test]
        public async Task GetDkimStatesToUpdateWhenNoOldRecordsTest()
        {
            DkimSchedulerState record = new DkimSchedulerState("abc.com");

            DkimSchedulerState record2 = new DkimSchedulerState("def.com");

            await _dkimSchedulerDao.Save(record);

            await _dkimSchedulerDao.Save(record2);

            var results = await _dkimPeriodicSchedulerDao.GetDkimRecordsToUpdate();

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetDkimStatesToUpdateWhenHasOldRecordsTest()
        {
            DkimSchedulerState record = new DkimSchedulerState("abc.com");

            DkimSchedulerState record2 = new DkimSchedulerState("def.com");

            await _dkimSchedulerDao.Save(record);

            await _dkimSchedulerDao.Save(record2);

            await UpdateLastChecked(2);

            var results = await _dkimPeriodicSchedulerDao.GetDkimRecordsToUpdate();

            Assert.That(results.Count, Is.EqualTo(2));

            //first record
            Assert.That(results.First().Id, Is.EqualTo(record.Id));

            //last record
            Assert.That(results.Last().Id, Is.EqualTo(record2.Id));
        }

        protected override string GetDatabaseName()
        {
            return "dkimscheduler";
        }

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        private async Task UpdateLastChecked(int days)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                $"UPDATE dkim_scheduled_records SET last_checked = DATE(NOW() - INTERVAL {days} DAY);");
        }
    }
}