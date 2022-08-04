using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Entity;
using NUnit.Framework;

namespace MailCheck.Dkim.Entity.Test.Entity
{
    [TestFixture]
    public class DkimEntityStateTests
    {
        private const string Domain = "abc.com";
        private const string Selector1 = "selector1";
        private const string Selector2 = "selector2";
        private const string Selector3 = "selector3";
        private const string Dkim1 = "v = DKIM1....1";
        private const string Dkim2 = "v = DKIM1....2";
        private const string Dkim3 = "v = DKIM1....3";

        [Test]
        public void UpdateSelectorsWhenSelectorsEmptyAddsSelectors()
        {
            DkimEntityState state = CreateState(Domain, DkimState.Evaluated);

            DkimSelector selector1 = CreateSelector(Selector1);
            DkimSelector selector2 = CreateSelector(Selector2);

            bool updated = state.UpdateSelectors(new List<DkimSelector> { selector1, selector2 },
                 out DkimSelectorsUpdated dkimSelectorsUpdated);

            Assert.That(updated, Is.True);

            Assert.That(state.Selectors.Count == 2);
            Assert.That(state.Selectors[0], Is.EqualTo(selector1));
            Assert.That(state.Selectors[1], Is.EqualTo(selector2));

            Assert.That(dkimSelectorsUpdated.Selectors.Count == 2);
            Assert.That(dkimSelectorsUpdated.Selectors[0] == selector1.Selector);
            Assert.That(dkimSelectorsUpdated.Selectors[1] == selector2.Selector);
        }

        [Test]
        public void UpdateSelectorWhenNewSelectorsAddsNewSelectors()
        {
            DkimEntityState state = CreateState(Domain, DkimState.Evaluated);

            DkimSelector selector1 = CreateSelector(Selector1);
            DkimSelector selector2 = CreateSelector(Selector2);
            DkimSelector selector3 = CreateSelector(Selector3);

            state.UpdateSelectors(new List<DkimSelector> { selector1, selector2 }, out DkimSelectorsUpdated dkimSelectorsUpdated);

            bool updated = state.UpdateSelectors(new List<DkimSelector> { selector3 }, out dkimSelectorsUpdated);

            Assert.That(updated, Is.True);

            Assert.That(state.Selectors.Count == 3);
            Assert.That(state.Selectors[0], Is.EqualTo(selector1));
            Assert.That(state.Selectors[1], Is.EqualTo(selector2));
            Assert.That(state.Selectors[2], Is.EqualTo(selector3));

            Assert.That(dkimSelectorsUpdated.Selectors.Count == 3);
            Assert.That(dkimSelectorsUpdated.Selectors[0] == selector1.Selector);
            Assert.That(dkimSelectorsUpdated.Selectors[1] == selector2.Selector);
            Assert.That(dkimSelectorsUpdated.Selectors[2] == selector3.Selector);
        }

        [Test]
        public void UpdateSelectorsWhenNoNewSelectorsNothingChanges()
        {
            DkimEntityState state = CreateState(Domain, DkimState.Evaluated);

            DkimSelector selector1 = CreateSelector(Selector1);
            DkimSelector selector2 = CreateSelector(Selector2);

            state.UpdateSelectors(new List<DkimSelector> { selector1, selector2 }, out DkimSelectorsUpdated _);

            bool updated = state.UpdateSelectors(new List<DkimSelector> { selector1, selector2 }, out _);

            Assert.That(updated, Is.False);

            Assert.That(state.Selectors.Count == 2);
            Assert.That(state.Selectors[0], Is.EqualTo(selector1));
            Assert.That(state.Selectors[1], Is.EqualTo(selector2));
        }
        
        [Test]
        public void UpdateRecordsClearsExistingRecordsAndUpdatesWithNewOnes()
        {
            DkimSelector selector1 = CreateSelector(Selector1);
            DkimSelector selector2 = CreateSelector(Selector2);

            DkimEntityState state = CreateState(Domain, DkimState.PollPending, selector1, selector2);

            DkimSelector selector3 = CreateSelector(Selector1, CreateRecord(Dkim1));

            state.UpdateRecords(new List<DkimSelector> { selector3 }, DateTime.UtcNow);

            Assert.That(state.Selectors[0].Selector, Is.EqualTo(selector3.Selector));

            Assert.That(state.Selectors.Count, Is.EqualTo(1));
            Assert.That(state.Selectors[0].Records.Count, Is.EqualTo(1));
            Assert.That(state.Selectors[0].Records[0].DnsRecord.Record, Is.EqualTo(Dkim1));
            Assert.That(state.Selectors[0].PollError, Is.Null);
        }
        
        [Test]
        public void UpdateRecordsWhenNoRecordsFromPollUpdatesErrorWhenNoErrorsExist()
        {
            DkimSelector selector1 = CreateSelector(Selector1);

            DkimEntityState state = CreateState(Domain, DkimState.PollPending, selector1);

            DkimSelector selector3 = new DkimSelector(Selector1, new List<DkimRecord>(), null);

            state.UpdateRecords(new List<DkimSelector> { selector3 }, DateTime.UtcNow);

            Assert.That(state.Selectors[0].Selector, Is.EqualTo(selector3.Selector));

            Assert.That(state.Selectors.Count, Is.EqualTo(1));
            Assert.That(state.Selectors[0].Records.Count, Is.EqualTo(0));
            StringAssert.StartsWith("Didn't find any DNS TXT", state.Selectors[0].PollError.Text);
        }              

        [Test]
        public void UpdatesWithNoChangesRecordEvaluationMessages()
        {
            DkimSelector selector1 = CreateSelector(Selector1, CreateRecord(Dkim1));

            DkimEntityState state = CreateState(Domain, DkimState.PollPending, selector1);

            DkimSelector selector2 = CreateSelector(Selector1, CreateRecord(Dkim1));

            state.UpdateRecords(new List<DkimSelector> { selector2 }, DateTime.UtcNow);

            DkimEvaluationUpdated evaluationUpdated = state.UpdateEvaluations(DateTime.UtcNow);

            Assert.That(evaluationUpdated, Is.Not.Null);
        }

        [Test]
        public void UpdatesEmptyRecordEvaluationMessages()
        {
            DkimSelector selector1 = CreateSelector(Selector1, CreateRecord(Dkim1, new Message(Guid.NewGuid(), "mailcheck.dkim.testName", MessageType.warning, "oh boy!", string.Empty)));
            
            DkimEntityState state = CreateState(Domain, DkimState.PollPending, selector1);

            DkimSelector selector2 = CreateSelector(Selector1, CreateRecordWithEmptyList(Dkim1));

            state.UpdateRecords(new List<DkimSelector> { selector2 }, DateTime.UtcNow);

            DkimEvaluationUpdated evaluationUpdated = state.UpdateEvaluations(DateTime.UtcNow);

            Assert.AreEqual(evaluationUpdated.DkimEvaluationResults[0].Records[0].EvaluationMessages.Count, 0);
        }

        private DkimEntityState CreateState(string domain, DkimState dkimState, params DkimSelector[] selectors)
        {
            return new DkimEntityState(domain, 1, dkimState, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, selectors.Any() ? selectors.ToList() : null);
        }

        private DkimSelector CreateSelector(string selector, params DkimRecord[] records)
        {
            return CreateSelector(selector, null, records);
        }

        private DkimSelector CreateSelector(string selector, Message pollMessage, params DkimRecord[] records)
        {
            return new DkimSelector(selector, records.Any() ? records.ToList() : null, null, pollMessage);
        }

        private DkimRecord CreateRecord(string record, params Message[] messages)
        {
            return new DkimRecord(new DnsRecord(record, new List<string> { record }), messages.Any() ? messages.ToList() : null);
        }

        private DkimRecord CreateRecordWithEmptyList(string record)
        {
            return new DkimRecord(new DnsRecord(record, new List<string> { record }), new List<Message>());
        }

    }
}
