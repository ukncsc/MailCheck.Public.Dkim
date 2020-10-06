using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using NUnit.Framework;
using DkimSelector = MailCheck.Dkim.Entity.Entity.DkimSelector;

namespace MailCheck.Dkim.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class RecordChangedNotifierTests
    {
        private RecordChangedNotifier _recordChangedNotifier;
        private Fixture _fixture;
        private IMessageDispatcher _messageDispatcher;
        private IDkimEntityConfig _dkimEntityConfig;
        private IEqualityComparer<SelectorRecord> _selectorRecordEqualityComparer;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _dkimEntityConfig = A.Fake<IDkimEntityConfig>();
            _selectorRecordEqualityComparer = new SelectorRecordEqualityComparer();

            _fixture = new Fixture();
            _recordChangedNotifier = new RecordChangedNotifier(_messageDispatcher, _dkimEntityConfig, _selectorRecordEqualityComparer);
        }

        [Test]
        public void DoesNotNotifyWhenNoChanges()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record"}}
            };
            Dictionary<string, List<string>> selectorsAndRecords2 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record"}}
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = CreateDkimRecordsPolled(selectorsAndRecords2);

            _recordChangedNotifier.Handle(state, message);

            A.CallTo(() => _messageDispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void DoesNotNotifyWhenNewSelectorHasNoRecord()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record"}}
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = new DkimRecordsPolled("", new List<DkimSelectorRecords>
            {
                new DkimSelectorRecords(Guid.NewGuid(),"selector1", new List<DkimTxtRecord>(){new DkimTxtRecord(new List<string>{"record"})}, null, 0),
                new DkimSelectorRecords(Guid.NewGuid(), "rogueSelector", null, null, 0)
            });

            _recordChangedNotifier.Handle(state, message);

            A.CallTo(() => _messageDispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenSelectorChanges()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1"}}
            };

            Dictionary<string, List<string>> selectorsAndRecords2 = new Dictionary<string, List<string>>
            {
                {"selector2", new List<string>{"record1"}}
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = CreateDkimRecordsPolled(selectorsAndRecords2);

            _recordChangedNotifier.Handle(state, message);
            
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordAdded>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector2" && x.SelectorRecords[0].Records[0] == "record1"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordRemoved>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector1" && x.SelectorRecords[0].Records[0] == "record1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenRecordsAdded()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1"}}
            };

            Dictionary<string, List<string>> selectorsAndRecords2 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1", "record2", "record3"}},
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = CreateDkimRecordsPolled(selectorsAndRecords2);

            _recordChangedNotifier.Handle(state, message);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordRemoved>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordAdded>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector1" && x.SelectorRecords[0].Records[0] == "record2" && x.SelectorRecords[0].Records[1] == "record3"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenRecordsRemoved()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1", "record2", "record3"}}
            };

            Dictionary<string, List<string>> selectorsAndRecords2 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1"}},
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = CreateDkimRecordsPolled(selectorsAndRecords2);

            _recordChangedNotifier.Handle(state, message);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordRemoved>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector1" && x.SelectorRecords[0].Records[0] == "record2" && x.SelectorRecords[0].Records[1] == "record3"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenRecordChanges()
        {
            Dictionary<string, List<string>> selectorsAndRecords1 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1", "record2"}}
            };

            Dictionary<string, List<string>> selectorsAndRecords2 = new Dictionary<string, List<string>>
            {
                {"selector1", new List<string>{"record1", "record3"}}
            };

            DkimEntityState state = CreateDkimEntityState(selectorsAndRecords1);
            DkimRecordsPolled message = CreateDkimRecordsPolled(selectorsAndRecords2);

            _recordChangedNotifier.Handle(state, message);

            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordAdded>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector1" && x.SelectorRecords[0].Records[0] == "record3"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() =>
                _messageDispatcher.Dispatch(
                    A<DkimRecordRemoved>.That.Matches(x =>
                        x.SelectorRecords[0].Selector == "selector1" && x.SelectorRecords[0].Records[0] == "record2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        private DkimEntityState CreateDkimEntityState(Dictionary<string, List<string>> selectorsAndRecords)
        {
            List<DkimSelector> dkimSelectors = new List<DkimSelector>();
            foreach (KeyValuePair<string, List<string>> kvp in selectorsAndRecords)
            {
                List<DkimRecord> dkimRecords = kvp.Value.Select(record => new DkimRecord(new DnsRecord(record, null)) ).ToList();
                dkimSelectors.Add(new DkimSelector(kvp.Key, dkimRecords));
            }

            _fixture.Register(() => dkimSelectors);
            DkimEntityState dkimEntityState = _fixture.Create<DkimEntityState>();
            return dkimEntityState;
        }

        private DkimRecordsPolled CreateDkimRecordsPolled(Dictionary<string, List<string>> selectorsAndRecords)
        {
            List<DkimSelectorRecords> dkimSelectorRecords = new List<DkimSelectorRecords>();
            foreach (KeyValuePair<string, List<string>> kvp in selectorsAndRecords)
            {
                List<DkimTxtRecord> dkimTxtRecords = kvp.Value.Select(record => new DkimTxtRecord(record.Split(" ").ToList())).ToList();
                dkimSelectorRecords.Add(new DkimSelectorRecords(Guid.NewGuid(), kvp.Key, dkimTxtRecords, null, 0));
            }

            _fixture.Register(() => dkimSelectorRecords);
            DkimRecordsPolled dkimRecordsPolled = _fixture.Create<DkimRecordsPolled>();
            return dkimRecordsPolled;
        }
    }
}
