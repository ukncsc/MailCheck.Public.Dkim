using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Scheduler;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MailCheck.Dkim.Scheduler.Processor;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dkim.Scheduler.Test.Processor
{
    [TestFixture]
    public class DkimPollSchedulerProcessorTests
    {
        private IDkimPeriodicSchedulerDao _dao;
        private IMessagePublisher _publisher;
        private IDkimSchedulerConfig _config;
        private ILogger<DkimPollSchedulerProcessor> _log;
        private DkimPollSchedulerProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<IDkimPeriodicSchedulerDao>();
            _publisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<IDkimSchedulerConfig>();
            _log = A.Fake<ILogger<DkimPollSchedulerProcessor>>();

            _processor = new DkimPollSchedulerProcessor(_dao, _publisher, _config, _log);
        }

        [Test]
        public async Task ProcessWithOneMessageTest()
        {
            DkimSchedulerState dkimSchedulerState = new DkimSchedulerState("abc.com");
            List<DkimSchedulerState> records = new List<DkimSchedulerState>
            {
                dkimSchedulerState
            };

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).Returns(records);

            ProcessResult result = await _processor.Process();

            Assert.That(result.ContinueProcessing, Is.True);

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable< DkimSchedulerState>>.That.Contains(dkimSchedulerState)))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ProcessWithoutAnyMessagesTest()
        {
            ProcessResult result = await _processor.Process();

            Assert.That(result.ContinueProcessing, Is.False);

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<DkimSchedulerState>>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task ProcessWithFifteenMessageTest()
        {
            DkimSchedulerState dkimSchedulerState = new DkimSchedulerState("abc.com");
            List<DkimSchedulerState> records = Enumerable.Repeat(dkimSchedulerState, 15).ToList();

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).Returns(records);

            ProcessResult result = await _processor.Process();

            Assert.That(result.ContinueProcessing, Is.True);

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .MustHaveHappened(15, Times.Exactly);

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.That.IsSameSequenceAs(Enumerable.Repeat(dkimSchedulerState, 10))))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.That.IsSameSequenceAs(Enumerable.Repeat(dkimSchedulerState, 5))))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ProcessWithFifteenMessageAndOneFailedPublishTest()
        {
            DkimSchedulerState dkimSchedulerState = new DkimSchedulerState("abc.com");
            List<DkimSchedulerState> records = Enumerable.Repeat(dkimSchedulerState, 15).ToList();

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).Returns(records);

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .Throws<Exception>().Once()
                .Then.Returns(Task.CompletedTask);

            ProcessResult result = await _processor.Process();

            Assert.That(result.ContinueProcessing, Is.True);

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .MustHaveHappened(6, Times.Exactly);

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.That.IsSameSequenceAs(Enumerable.Repeat(dkimSchedulerState, 5))))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ProcessWithFifteenMessageAndOneFailedSaveTest()
        {
            DkimSchedulerState dkimSchedulerState = new DkimSchedulerState("abc.com");
            List<DkimSchedulerState> records = Enumerable.Repeat(dkimSchedulerState, 15).ToList();

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).Returns(records);

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .Returns(Task.CompletedTask);

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>._))
               .Returns(Task.FromException<Exception>(new Exception("Test")));

            ProcessResult result = await _processor.Process();

            Assert.That(result.ContinueProcessing, Is.False);

            A.CallTo(() => _dao.GetDkimRecordsToUpdate()).MustHaveHappenedOnceExactly();

            A.CallTo(() => _publisher.Publish(A<DkimRecordsExpired>._, A<string>._))
                .MustHaveHappened(10, Times.Exactly);

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.That.IsSameSequenceAs(Enumerable.Repeat(dkimSchedulerState, 10))))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<IEnumerable<DkimSchedulerState>>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
