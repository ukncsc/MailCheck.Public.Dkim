using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Entity.DomainStatus;
using MailCheck.DomainStatus.Contracts;
using NUnit.Framework;

namespace MailCheck.Dkim.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusEvaluatorTests
    {
        private DomainStatusEvaluator _domainStatusEvaluator;

        [SetUp]
        public void SetUp()
        {
            _domainStatusEvaluator = new DomainStatusEvaluator();
        }

        [TestCase(Status.Success, null)]
        [TestCase(Status.Success, new MessageType[] { })]
        [TestCase(Status.Info, new[] { MessageType.info, MessageType.info })]
        [TestCase(Status.Warning, new[] { MessageType.info, MessageType.warning })]
        [TestCase(Status.Warning, new[] { MessageType.warning, MessageType.warning })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.warning, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.error, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.warning, MessageType.error })]
        public void StatusDeterminedByMessages(Status expectedStatus, MessageType[] messageTypes)
        {
            List<Message> messages = messageTypes?.Select(CreateMessage).ToList();

            Status result = _domainStatusEvaluator.GetStatus(null, messages);

            Assert.AreEqual(result, expectedStatus);
        }

        [TestCase(Status.Success, null)]
        [TestCase(Status.Success, new MessageType[] { })]
        [TestCase(Status.Info, new[] { EvaluationErrorType.Info, EvaluationErrorType.Info })]
        [TestCase(Status.Warning, new[] { EvaluationErrorType.Info, EvaluationErrorType.Warning })]
        [TestCase(Status.Warning, new[] { EvaluationErrorType.Warning, EvaluationErrorType.Warning })]
        [TestCase(Status.Error, new[] { EvaluationErrorType.Info, EvaluationErrorType.Error })]
        [TestCase(Status.Error, new[] { EvaluationErrorType.Warning, EvaluationErrorType.Error })]
        [TestCase(Status.Error, new[] { EvaluationErrorType.Error, EvaluationErrorType.Error })]
        [TestCase(Status.Error, new[] { EvaluationErrorType.Info, EvaluationErrorType.Warning, EvaluationErrorType.Error })]
        public void StatusDeterminedByEvaluatorMessages(Status expectedStatus, EvaluationErrorType[] messageTypes)
        {
            List<DkimEvaluatorMessage> messages = messageTypes?.Select(CreateDkimEvaluatorMessage).ToList();

            Status result = _domainStatusEvaluator.GetStatus(messages, null);

            Assert.AreEqual(result, expectedStatus);
        }

        [TestCase(Status.Error, EvaluationErrorType.Error, MessageType.warning)]
        [TestCase(Status.Error, EvaluationErrorType.Warning, MessageType.error)]
        [TestCase(Status.Warning, EvaluationErrorType.Warning, MessageType.info)]
        [TestCase(Status.Warning, EvaluationErrorType.Info, MessageType.warning)]
        [TestCase(Status.Info, EvaluationErrorType.Info, MessageType.info)]
        public void StatusDeterminedByBothTypesOfMessage(Status expectedStatus, EvaluationErrorType evaluatorMessageType, MessageType messageType)
        {
            List<DkimEvaluatorMessage> evaluatorMessages = new List<DkimEvaluatorMessage> { CreateDkimEvaluatorMessage(evaluatorMessageType) };

            List<Message> messages = new List<Message> { CreateMessage(messageType) };

            Status result = _domainStatusEvaluator.GetStatus(evaluatorMessages, messages);

            Assert.AreEqual(result, expectedStatus);
        }

        private DkimEvaluatorMessage CreateDkimEvaluatorMessage(EvaluationErrorType messageType)
        {
            return new DkimEvaluatorMessage(Guid.Empty, "mailcheck.dkim.testName", messageType, "", "");
        }

        private Message CreateMessage(MessageType messageType)
        {
            return new Message(Guid.Empty, "mailcheck.dkim.testName", messageType, "", "");
        }
    }
}
