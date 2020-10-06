using System;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Dao;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Service;
using NUnit.Framework;

namespace MailCheck.Dkim.Api.Test.DkimServiceTests
{
    [TestFixture]
    public class DkimServiceTests
    {
        private DkimService _dkimService;
        private IMessagePublisher _messagePublisher;
        private IDkimApiDao _dao;
        private IDkimApiConfig _config;
        private IClock _clock;

        [SetUp]
        public void SetUp()
        {
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<IDkimApiDao>();
            _config = A.Fake<IDkimApiConfig>();
            _clock = A.Fake<IClock>();
            _dkimService = new DkimService(_messagePublisher, _dao, _config, _clock);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainDoesNotExist()
        {
            A.CallTo(() => _dao.GetDkimSelectors("testDomain"))
                .Returns(Task.FromResult<EntityDkimEntityState>(null));

            EntityDkimEntityState result = await _dkimService.GetDkimForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainExistsButRecordsNeverUpdated()
        {
            EntityDkimEntityState dkimInfoResponse = GetEntityDkimEntityState(null);

            A.CallTo(() => _dao.GetDkimSelectors("testDomain"))
                .Returns(Task.FromResult(dkimInfoResponse));

            EntityDkimEntityState result = await _dkimService.GetDkimForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreSame(dkimInfoResponse, result);
        }


        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainExistsAndRecordsStale()
        {
            EntityDkimEntityState dkimInfoResponse = GetEntityDkimEntityState(DateTime.UnixEpoch.AddDays(-2));

            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.UnixEpoch);
            A.CallTo(() => _dao.GetDkimSelectors("testDomain")).Returns(Task.FromResult(dkimInfoResponse));

            EntityDkimEntityState result = await _dkimService.GetDkimForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreSame(dkimInfoResponse, result);
        }

        [Test]
        public async Task DoesNotPublishDomainMissingMessageWhenDomainExistsAndRecordsNotStale()
        {
            EntityDkimEntityState dkimInfoResponse = GetEntityDkimEntityState(DateTime.UnixEpoch.AddDays(-2).AddTicks(1));

            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.UnixEpoch);
            A.CallTo(() => _dao.GetDkimSelectors("testDomain")).Returns(Task.FromResult(dkimInfoResponse));

            EntityDkimEntityState result = await _dkimService.GetDkimForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustNotHaveHappened();
            Assert.AreSame(dkimInfoResponse, result);
        }

        private EntityDkimEntityState GetEntityDkimEntityState(DateTime? recordsLastUpdated)
        {
            return new EntityDkimEntityState("", EntityDkimState.Created, 0, DateTime.MaxValue, recordsLastUpdated, null, null);
        }
    }
}