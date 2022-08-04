using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using NUnit.Framework;
using Message = MailCheck.Dkim.Contracts.SharedDomain.Message;

namespace MailCheck.Dkim.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class AdvisoryChangedNotifierTests
    {
        private AdvisoryChangedNotifier _advisoryChangedNotifier;
        private IMessageDispatcher _messageDispatcher;
        private IDkimEntityConfig _dkimEntityConfig;
        private IEqualityComparer<SelectorMessage> _selectorMessageEqualityComparer;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _dkimEntityConfig = A.Fake<IDkimEntityConfig>();
            _selectorMessageEqualityComparer = new SelectorMessageEqualityComparer();
            _advisoryChangedNotifier = new AdvisoryChangedNotifier(_messageDispatcher, _dkimEntityConfig, _selectorMessageEqualityComparer);
        }

        [Test]
        public void RaisesAdvisorySustainedOnlyWhenNoChange()
        {
            Guid Id1 = Guid.NewGuid();
            Guid Id2 = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty),
                    new Message(Id2, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Id2, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisorySustainedOnlyWhenNewSelectorHasNoRecord()
        {
            Guid Id = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1 same as old", string.Empty)
                }),

                new DkimSelectorResult(new Contracts.SharedDomain.DkimSelector( "rogueSelector"), null)
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesOnlyAdvisorySustainedEvenWhenMessageChanges()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty),
                    new Message(Guid.NewGuid(), "mailcheck.dkim.testName2", MessageType.info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Guid.NewGuid(), "mailcheck.dkim.testName2", EvaluationErrorType.Info, "selector 1 record 1 message 3", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);
            
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisorySustainedAndReoveAdvisoryWhenMessageChanges()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty),
                    new Message(Guid.NewGuid(), "mailcheck.dkim.testName2", MessageType.info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryRemoved>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisorySustainedAndAddAdvisoryWhenMessageAdded()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Guid.NewGuid(), "mailcheck.dkim.testName2", EvaluationErrorType.Info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x => 
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryAdded>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisorySustainedAndAddAdvisoryWhenSelectorAdded()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                }),
                CreateDkimSelectorResult("selector2", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Guid.NewGuid(), "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 2 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryAdded>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector2" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 2 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAdvisorySustainedAndRemoveAdvisoryWhenSelectorRemoved()
        {
            Guid Id1 = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "mailcheck.dkim.testName", MessageType.info, "selector 1 record 1 message 1",  string.Empty)
                }),
                CreateDkimSelector("selector2", new List<Message>
                {
                    new Message(Guid.NewGuid(), "mailcheck.dkim.testName2", MessageType.info, "selector 2 record 1 message 1", string.Empty)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryRemoved>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector2" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 2 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisorySustained>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NoDkimAdvisorySustainedWhenChangeHasHappened()
        {
            Guid Id1 = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue,
                DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
                {
                    CreateDkimSelector("selector2", new List<Message>
                    {
                        new Message(Guid.NewGuid(), "mailcheck.dkim.testName", MessageType.info, "selector 2 record 1 message 1", string.Empty)
                    })
                });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1, "mailcheck.dkim.testName", EvaluationErrorType.Info, "selector 1 record 1 message 1",
                        string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisorySustained>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() =>
                    _messageDispatcher.Dispatch(
                        A<DkimAdvisoryAdded>.That.Matches(x =>
                            x.SelectorMessages[0].Selector == "selector1" &&
                            (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                            x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 1"), A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                    _messageDispatcher.Dispatch(
                        A<DkimAdvisoryRemoved>.That.Matches(x =>
                            x.SelectorMessages[0].Selector == "selector2" &&
                            (MessageType) x.SelectorMessages[0].Messages[0].MessageType == MessageType.info &&
                            x.SelectorMessages[0].Messages[0].Text == "selector 2 record 1 message 1"), A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        private DkimSelector CreateDkimSelector(string selector, List<Message> messages)
        {
            return new DkimSelector(selector, new List<DkimRecord>
            {
                new DkimRecord(null, messages)
            });
        }

        private DkimSelectorResult CreateDkimSelectorResult(string selector, List<DkimEvaluatorMessage> messages)
        {
            return new DkimSelectorResult(new Contracts.SharedDomain.DkimSelector(selector), new List<RecordResult>
            {
                new RecordResult(null, messages)
            });
        }
    }
}
