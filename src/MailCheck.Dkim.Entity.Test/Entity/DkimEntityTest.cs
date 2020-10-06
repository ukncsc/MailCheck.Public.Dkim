using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.Common.Util;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.External;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Dao;
using MailCheck.Dkim.Entity.Entity;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Entity.Entity.DomainStatus;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Dkim.Contracts.Scheduler;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Dkim.Entity.Test.Entity
{
    [TestFixture]
    public class DkimEntityTest
    {
        private const string Id = "abc.com";

        private DkimEntity _dkimEntity;
        private IDkimEntityDao _dkimEntityDao;
        private IDkimEntityConfig _dkimEntityConfig;
        private ILogger<DkimEntity> _logger;
        private IMessageDispatcher _dispatcher;
        private IDomainStatusPublisher _domainStatusPublisher;
        private IChangeNotifiersComposite _changeNotifierComposite;
        private IClock _clock;

        [SetUp]
        public void SetUp()
        {
            _dkimEntityDao = A.Fake<IDkimEntityDao>();
            _dkimEntityConfig = A.Fake<IDkimEntityConfig>();
            _logger = A.Fake<ILogger<DkimEntity>>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _domainStatusPublisher = A.Fake<IDomainStatusPublisher>();
            _changeNotifierComposite = A.Fake<IChangeNotifiersComposite>();
            _clock = A.Fake<IClock>();

            _dkimEntity = new DkimEntity(_dkimEntityDao, _dkimEntityConfig, _dispatcher, _changeNotifierComposite, _domainStatusPublisher, _clock, _logger);
        }

        [Test]
        public async Task HandleCreateDomainCreatesDomain()
        {
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns<DkimEntityState>(null);

            await _dkimEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<DkimEntityCreated>.That.Matches(x=>x.State == DkimState.Created && x.Id == Id), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HandleCreateDomainThrowsIfAlreadyExists()
        {
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(new DkimEntityState(Id, 1, DkimState.Created, DateTime.UnixEpoch, DateTime.UnixEpoch, DateTime.UnixEpoch, new List<DkimSelector>()));

            Assert.ThrowsAsync<MailCheckException>(() => _dkimEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now)));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<DkimEntityCreated>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleCreateDomainPublishesIfStale()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.UnixEpoch);
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(new DkimEntityState(Id, 1, DkimState.Created, DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(-2), DateTime.UnixEpoch, new List<DkimSelector>()));

            await _dkimEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<DkimEntityCreated>.That.Matches(x => x.State == DkimState.Created && x.Id == Id), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainDeletedDeletesDomain()
        {
            await _dkimEntity.Handle(new DomainDeleted(Id));

            A.CallTo(() => _dkimEntityDao.Delete(A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleAggregateReportDomainDkimSelectorsStateExistsAndEventCausesUpdate()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(true);

            DkimSelectorsUpdated dkimSelectorsUpdated;
            A.CallTo(() => dkimEntityState.UpdateSelectors(null, out dkimSelectorsUpdated)).WithAnyArguments().Returns(true);
            
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(new DkimSelectorsSeen("", "", Id, new List<string>()));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleAggregateReportDomainDkimSelectorsStateExistsButEventDoesntCauseUpdate()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(true);

            DkimSelectorsUpdated dkimSelectorsUpdated;
            A.CallTo(() => dkimEntityState.UpdateSelectors(null, out dkimSelectorsUpdated)).WithAnyArguments().Returns(false);

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(new DkimSelectorsSeen("", "", Id, new List<string>()));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
        }

        [Test]
        public void HandleAggregateReportDomainDkimSelectorsNoStateExistsThrows()
        {
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns<DkimEntityState>(null);

            Assert.ThrowsAsync<MailCheckException>(() => _dkimEntity.Handle(new DkimSelectorsSeen("", "", Id, new List<string>())));
            A.CallTo(() => _dispatcher.Dispatch(A<DomainMissing>.That.Matches(x => x.Id == Id), A<string>._)).MustHaveHappenedOnceExactly();

        }

        [Test]
        public async Task HandleAggregateReportDomainDkimSelectorsCantUpdateDoesNotContinue()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(false);

            DkimSelectorsUpdated dkimSelectorsUpdated;
            A.CallTo(() => dkimEntityState.UpdateSelectors(null, out dkimSelectorsUpdated)).WithAnyArguments().Returns(false);

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(new DkimSelectorsSeen("", "", Id, new List<string>()));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void HandleDkimRecordsPolledNoStateExistsThrows()
        {
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns<DkimEntityState>(null);

            Assert.ThrowsAsync<MailCheckException>(() => _dkimEntity.Handle(new DkimRecordEvaluationResult(Id, new List<DkimSelectorResult>())));
        }

        [Test]
        public async Task HandleDkimRecordsPolledCantUpdateDoesNotContinue()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(false);

            DkimSelectorsUpdated dkimSelectorsUpdated;
            A.CallTo(() => dkimEntityState.UpdateSelectors(null, out dkimSelectorsUpdated)).WithAnyArguments().Returns(false);

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(new DkimRecordEvaluationResult(Id, new List<DkimSelectorResult>()));

            A.CallTo(() => _changeNotifierComposite.Handle(A<DkimEntityState>._, A<Message>._)).MustNotHaveHappened();
            A.CallTo(() => _domainStatusPublisher.Publish(A<DkimRecordEvaluationResult>._)).MustNotHaveHappened();
            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }


        [Test]
        public async Task HandleDkimRecordEvaluationResultRemovesPolledErrorSelectors()
        {
            DkimRecordEvaluationResult message = new DkimRecordEvaluationResult(
                Id, new List<DkimSelectorResult>
                {
                    new DkimSelectorResult(new Contracts.SharedDomain.DkimSelector("selector", null, null, new Contracts.SharedDomain.Message(Guid.Empty, Contracts.SharedDomain.MessageType.error, "error", "markdown")), new List<RecordResult>())
                });

            message.Timestamp = DateTime.UtcNow;

            DkimEntityState dkimEntityState = new DkimEntityState(Id, 1, DkimState.PollPending, DateTime.Now, null, null, new List<DkimSelector>()); 
            
            dkimEntityState.Selectors.Add(new DkimSelector("selector1"));
            
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(message);

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>.That.Matches(x => x.Selectors.Count == 0))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDkimRecordEvaluationResultUpdatesPolledSelectors()
        {
            DkimRecordEvaluationResult message = new DkimRecordEvaluationResult(
                Id, new List<DkimSelectorResult>
                {
                    new DkimSelectorResult(new Contracts.SharedDomain.DkimSelector("selector1", null), new List<RecordResult>{

                    new RecordResult("record1", new List<DkimEvaluatorMessage>())
                    })
                });

            message.Timestamp = DateTime.UtcNow;

            DkimEntityState dkimEntityState = new DkimEntityState(Id, 1, DkimState.PollPending, DateTime.Now, null, null, new List<DkimSelector>());

            dkimEntityState.Selectors.Add(new DkimSelector("selector1"));

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(message);

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>.That.Matches(x => x.Selectors.Count == 1 && x.Selectors[0].Selector == "selector1"))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HandleDkimEvaluationResultNoStateExistsThrows()
        {
            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns<DkimEntityState>(null);

            Assert.ThrowsAsync<MailCheckException>(() => _dkimEntity.Handle(new DkimRecordEvaluationResult(Id, new List<DkimSelectorResult>())));
        }

        [Test]
        public async Task HandleDkimEvaluationUpdateSuccessful()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(true);

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            DkimRecordEvaluationResult dkimRecordEvaluationResult = new DkimRecordEvaluationResult(Id, new List<DkimSelectorResult>());
            await _dkimEntity.Handle(dkimRecordEvaluationResult);

            A.CallTo(() => _dkimEntityDao.Save(dkimEntityState)).MustHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<DkimEvaluationUpdated>._, A<string>._)).MustHaveHappened();
        }

        [Test]
        public async Task HandleDkimEvaluationUpdateCantUpdateDoesNotContinue()
        {
            DkimEntityState dkimEntityState = A.Fake<DkimEntityState>();
            A.CallTo(() => dkimEntityState.Id).Returns(Id);
            A.CallTo(() => dkimEntityState.CanUpdate(A<string>._, A<DateTime>._)).Returns(false);

            A.CallTo(() => _dkimEntityDao.Get(Id)).Returns(dkimEntityState);

            await _dkimEntity.Handle(new DkimRecordEvaluationResult(Id, new List<DkimSelectorResult>()));

            A.CallTo(() => _dkimEntityDao.Save(A<DkimEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }
    }
}
