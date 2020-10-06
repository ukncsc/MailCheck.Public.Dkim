using System;
using System.Collections.Generic;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.EntityHistory.Entity;
using MailCheck.Dkim.EntityHistory.Mapping;
using NUnit.Framework;

namespace MailCheck.Dkim.EntityHistory.Test.Mapping
{
    [TestFixture]
    public class HistoryMappingTests
    {
        private const string Domain1 = "Domain1";
        private const string Selector1 = "Selector1";
        private const string DkimTxtRecord1 = "DkimTxtRecord1";

        private static readonly DateTime DateTime1 = new DateTime(2018, 12, 12);

        [TestCaseSource("TestData")]
        public void Test(DkimEvaluationUpdated input, DkimHistoryRecord expected)
        {
            DkimHistoryRecord actual = input.ToDkimHistoryRecord();
            Assert.That(actual.StartDate, Is.EqualTo(expected.StartDate));
            Assert.That(actual.EndDate, Is.EqualTo(expected.EndDate));
            CollectionAssert.AreEqual(actual.Entries, expected.Entries);
        }

        public static IEnumerable<TestCaseData> TestData()
        {
            yield return new TestCaseData(
                    new DkimEvaluationUpdated(Domain1, 1, null, DateTime.UtcNow) { Timestamp = DateTime1 },
                    new DkimHistoryRecord(DateTime1, null, new List<DkimHistoryRecordEntry>()))
                .SetName("Mapping handles null DkimEvaluationResults correctly");

            yield return new TestCaseData(
                    new DkimEvaluationUpdated(Domain1, 1, new List<Contracts.Entity.Domain.DkimEvaluationResults>(), DateTime.UtcNow) { Timestamp = DateTime1 },
                    new DkimHistoryRecord(DateTime1, null, new List<DkimHistoryRecordEntry>()))
                .SetName("Mapping handles empty DkimEvaluationResults correctly");

            yield return new TestCaseData(
                    new DkimEvaluationUpdated(Domain1, 1, new List<Contracts.Entity.Domain.DkimEvaluationResults>
                    {
                        new Contracts.Entity.Domain.DkimEvaluationResults(Selector1, null)
                    }, DateTime.UtcNow)
                    { Timestamp = DateTime1 },
                    new DkimHistoryRecord(DateTime1, null, new List<DkimHistoryRecordEntry>()))
                .SetName("Mapping handles empty DkimEvaluationResults correctly");

            yield return new TestCaseData(
                    new DkimEvaluationUpdated(Domain1, 1, new List<Contracts.Entity.Domain.DkimEvaluationResults>
                    {
                        new Contracts.Entity.Domain.DkimEvaluationResults(Selector1, new List<Contracts.Entity.Domain.DkimEvaluationRecord>())
                    }, DateTime.UtcNow)
                    { Timestamp = DateTime1 },
                    new DkimHistoryRecord(DateTime1, null, new List<DkimHistoryRecordEntry>()))
                .SetName("Mapping handles empty DkimEvaluationResults correctly");
        }
    }
}
