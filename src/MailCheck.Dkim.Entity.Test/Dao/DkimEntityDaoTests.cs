using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Dkim.Entity.Dao;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Migration;
using NUnit.Framework;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Entity.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class DkimEntityDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";
        private const string Selector1 = "abc_selector1";

        private DkimEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new DkimEntityDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            DkimEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task InsertStateAndRetrieveStateCorrectly()
        {
            DkimState dkimState = DkimState.Evaluated;

            DkimEntityState state = new DkimEntityState(Id, 1, dkimState, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
                new List<DkimSelector> {new DkimSelector(Selector1)});

            await _dao.Save(state);

            DkimEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.Version, Is.EqualTo(state.Version));
            Assert.That(stateFromDatabase.Selectors.Count, Is.EqualTo(1));
            Assert.That(stateFromDatabase.Selectors[0].Selector, Is.EqualTo(Selector1));
            Assert.That(stateFromDatabase.State, Is.EqualTo(dkimState));
        }

        [Test]
        public async Task SaveStateAlreadyPersistedForVersionThrows()
        {
            DkimEntityState state = new DkimEntityState(Id, 1, DkimState.Evaluated, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow,
                new List<DkimSelector> {new DkimSelector(Selector1)});

            await _dao.Save(state);

            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(state));
            Assert.That(exception.Message,
                Is.EqualTo($"Didn\'t update DkimEntityState because version 1 has already been persisted."));
        }

        protected override string GetDatabaseName() => "dkimentity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }
    }
}
