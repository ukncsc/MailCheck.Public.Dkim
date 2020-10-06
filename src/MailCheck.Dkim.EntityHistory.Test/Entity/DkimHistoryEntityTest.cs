using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.EntityHistory.Entity;
using NUnit.Framework;

namespace MailCheck.Dkim.EntityHistory.Test.Entity
{
    [TestFixture]
    public class DkimHistoryEntityTest
    {
        private const string DkimRecord1 = "DkimRecord1";
        private const string DkimRecord2 = "DkimRecord2";
        private const string Selector1 = "Selector1";
        private const string Domain1 = "Domain1";

        [Test]
        public void UpdateEmptyHistoryWithIncomingRecordThatHasDkimRecordsCausesUpdate()
        {
            DkimHistoryEntityState state = new DkimHistoryEntityState(Domain1);

            DkimHistoryRecord dkimHistoryRecord = new DkimHistoryRecord(DateTime.Now, null,
                new List<DkimHistoryRecordEntry>
                {
                    new DkimHistoryRecordEntry(Selector1, new List<string>{DkimRecord1})
                });

            state.UpdateHistory(dkimHistoryRecord);

            Assert.That(state.Records.Count, Is.EqualTo(1));
            Assert.That(state.Records.First(), Is.EqualTo(dkimHistoryRecord));
        }

        [Test]
        public void UpdateHistoryWithIncomingRecordThatIsDifferentAndHasDkimRecordsCausesUpdate()
        {
            DateTime startDate1 = new DateTime(2018, 12, 12);
            DateTime startDate2 = new DateTime(2018, 12, 14);

            DkimHistoryRecord dkimHistoryRecord1 = new DkimHistoryRecord(startDate1, null,
                new List<DkimHistoryRecordEntry>
                {
                    new DkimHistoryRecordEntry(Selector1, new List<string>{DkimRecord1})
                });

            DkimHistoryEntityState state = new DkimHistoryEntityState(Domain1, new List<DkimHistoryRecord> { dkimHistoryRecord1 });

            DkimHistoryRecord dkimHistoryRecord2 = new DkimHistoryRecord(startDate2, null,
                new List<DkimHistoryRecordEntry>
                {
                    new DkimHistoryRecordEntry(Selector1, new List<string>{DkimRecord2})
                });

            state.UpdateHistory(dkimHistoryRecord2);

            Assert.That(state.Records.Count, Is.EqualTo(2));
            Assert.That(state.Records.Last().StartDate, Is.EqualTo(dkimHistoryRecord1.StartDate));
            Assert.That(state.Records.Last().EndDate, Is.EqualTo(dkimHistoryRecord2.StartDate));
            CollectionAssert.AreEqual(state.Records.Last().Entries, dkimHistoryRecord1.Entries);
            Assert.That(state.Records.First(), Is.EqualTo(dkimHistoryRecord2));
        }

        [Test]
        public void UpdateHistoryWithIncomingRecordThatIsNotDifferentAndHasDkimRecordsDoesntCauseUpdate()
        {
            DateTime startDate1 = new DateTime(2018, 12, 12);

            DkimHistoryRecord dkimHistoryRecord1 = new DkimHistoryRecord(startDate1, null,
                new List<DkimHistoryRecordEntry>
                {
                    new DkimHistoryRecordEntry(Selector1, new List<string>{DkimRecord1})
                });

            DkimHistoryEntityState state = new DkimHistoryEntityState(Domain1, new List<DkimHistoryRecord> { dkimHistoryRecord1 });

            state.UpdateHistory(dkimHistoryRecord1);

            Assert.That(state.Records.Count, Is.EqualTo(1));
            Assert.That(state.Records.First(), Is.EqualTo(dkimHistoryRecord1));
        }
    }
}
