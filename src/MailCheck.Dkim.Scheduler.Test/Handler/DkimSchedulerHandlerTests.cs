using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MailCheck.Dkim.Scheduler.Handler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dkim.Scheduler.Test.Handler
{
    [TestFixture]
    public class DkimSchedulerHandlerTests
    {
        private IDkimSchedulerDao _dao;
        private DkimSchedulerHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<IDkimSchedulerDao>();

            _handler = new DkimSchedulerHandler(_dao, A.Fake<ILogger<DkimSchedulerHandler>>());
        }

        [Test]
        public async Task DkimSchedulerItemDoesntExistDkimSchedulerItemIsCreatedOnDkimEntityCreatedMessage()
        {
            A.CallTo(() => _dao.Get(A<string>._))
                .Returns((DkimSchedulerState)null);

            DkimEntityCreated message = new DkimEntityCreated("Domain", 1);

            await _handler.Handle(message);

            A.CallTo(() => _dao.Save(A<DkimSchedulerState>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task DkimSchedulerItemExistsDkimSchedulerItemsIsNotCreatedDkimEntityCreatedMessage()
        {
            A.CallTo(() => _dao.Get(A<string>._))
                .Returns(new DkimSchedulerState(string.Empty));

            DkimEntityCreated message = new DkimEntityCreated("Domain", 1);

            await _handler.Handle(message);

            A.CallTo(() => _dao.Save(A<DkimSchedulerState>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task ItShouldDeleteTheSpfState()
        {
            await _handler.Handle(new DomainDeleted("ncsc.gov.uk"));

            A.CallTo(() => _dao.Delete(A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}
