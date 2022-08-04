using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity.DomainStatus;
using MailCheck.DomainStatus.Contracts;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Dkim.Contracts.SharedDomain.Message;

namespace MailCheck.Dkim.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusPublisherTests
    {
        private DomainStatusPublisher _domainStatusPublisher;
        private IMessageDispatcher _dispatcher;
        private IDkimEntityConfig _dmarcEntityConfig;
        private ILogger<DomainStatusPublisher> _logger;
        private IDomainStatusEvaluator _domainStatusEvaluator;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = A.Fake<IMessageDispatcher>();
            _dmarcEntityConfig = A.Fake<IDkimEntityConfig>();
            _logger = A.Fake<ILogger<DomainStatusPublisher>>();
            _domainStatusEvaluator = A.Fake<IDomainStatusEvaluator>();
            A.CallTo(() => _dmarcEntityConfig.SnsTopicArn).Returns("testSnsTopicArn");

            _domainStatusPublisher = new DomainStatusPublisher(_dispatcher, _dmarcEntityConfig, _domainStatusEvaluator, _logger);
        }

        [Test]
        public void StatusIsDeterminedAndDispatched()
        {
            Message pollError = CreateMessage();
            Message dkimRecordMessage = CreateMessage();
            DkimEvaluatorMessage dkimEvaluatorMessage = CreateDkimEvaluatorMessage();

            DkimRecordEvaluationResult message = new DkimRecordEvaluationResult("testDomain",
                new List<DkimSelectorResult>
                {
                    new DkimSelectorResult(
                        new DkimSelector("",
                            new List<DkimRecord> {new DkimRecord(null, new List<Message> {dkimRecordMessage})}, "",
                            pollError),
                        new List<RecordResult>
                            {new RecordResult("", new List<DkimEvaluatorMessage> {dkimEvaluatorMessage})})
                });

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<DkimEvaluatorMessage>>.That.Matches(x=>x.Contains(dkimEvaluatorMessage)), A<List<Message>>.That.Matches(x => x.Contains(pollError) && x.Contains(dkimRecordMessage)))).Returns(Status.Warning);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Warning &&
                x.RecordType == "DKIM" &&
                x.Id == "testDomain";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        private Message CreateMessage()
        {
            return new Message(Guid.Empty, "", MessageType.error, "", "");
        }

        private DkimEvaluatorMessage CreateDkimEvaluatorMessage()
        {
            return new DkimEvaluatorMessage(Guid.Empty, "mailcheck.dkim.testName", EvaluationErrorType.Error, "", "");
        }
    }
}
