using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using NUnit.Framework;
using Message = MailCheck.Dkim.Entity.Entity.Message;
using MessageType = MailCheck.Dkim.Entity.Entity.MessageType;

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
        public void DoesNotNotifyWhenNoChanges()
        {
            Guid Id1 = Guid.NewGuid();
            Guid Id2 = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1,"selector 1 record 1 message 1", string.Empty, MessageType.Info),
                    new Message(Id2,"selector 1 record 1 message 2", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Id2,EvaluationErrorType.Info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void DoesNotNotifyWhenNewSelectorHasNoRecord()
        {
            Guid Id = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id,"selector 1 record 1 message 1", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id,EvaluationErrorType.Info, "selector 1 record 1 message 1 same as old", string.Empty)
                }),

                new DkimSelectorResult(new Contracts.SharedDomain.DkimSelector( "rogueSelector"), null)
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenMessageChanges()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1,"selector 1 record 1 message 1", string.Empty, MessageType.Info),
                    new Message(Guid.NewGuid(),"selector 1 record 1 message 2", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Guid.NewGuid(),EvaluationErrorType.Info, "selector 1 record 1 message 3", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);
            
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryRemoved>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryAdded>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 3"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenMessageRemoved()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1,"selector 1 record 1 message 1", string.Empty, MessageType.Info),
                    new Message(Guid.NewGuid(),"selector 1 record 1 message 2", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryRemoved>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenMessageAdded()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1, "selector 1 record 1 message 1", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty),
                    new DkimEvaluatorMessage(Guid.NewGuid(),EvaluationErrorType.Info, "selector 1 record 1 message 2", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryAdded>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector1" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 1 record 1 message 2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenSelectorAdded()
        {
            Guid Id1 = Guid.NewGuid();
            
            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1,"selector 1 record 1 message 1", string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                }),
                CreateDkimSelectorResult("selector2", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Guid.NewGuid(),EvaluationErrorType.Info, "selector 2 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryAdded>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector2" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 2 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenSelectorRemoved()
        {
            Guid Id1 = Guid.NewGuid();

            DkimEntityState oldState = new DkimEntityState("", 0, DkimState.PollPending, DateTime.MinValue, DateTime.MinValue, DateTime.MaxValue, new List<DkimSelector>
            {
                CreateDkimSelector("selector1", new List<Message>
                {
                    new Message(Id1,"selector 1 record 1 message 1",  string.Empty, MessageType.Info)
                }),
                CreateDkimSelector("selector2", new List<Message>
                {
                    new Message(Guid.NewGuid(),"selector 2 record 1 message 1",string.Empty, MessageType.Info)
                })
            });

            DkimRecordEvaluationResult newRecord = new DkimRecordEvaluationResult(null, new List<DkimSelectorResult>
            {
                CreateDkimSelectorResult("selector1", new List<DkimEvaluatorMessage>
                {
                    new DkimEvaluatorMessage(Id1,EvaluationErrorType.Info, "selector 1 record 1 message 1", string.Empty)
                })
            });

            _advisoryChangedNotifier.Handle(oldState, newRecord);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimAdvisoryRemoved>.That.Matches(x =>
                        x.SelectorMessages[0].Selector == "selector2" && (MessageType)x.SelectorMessages[0].Messages[0].MessageType == MessageType.Info &&
                        x.SelectorMessages[0].Messages[0].Text == "selector 2 record 1 message 1"), A<string>._)).MustHaveHappenedOnceExactly();
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
