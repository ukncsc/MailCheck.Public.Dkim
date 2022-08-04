using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rsa;
using MailCheck.Dkim.Evaluator.Rules;
using MailCheck.Dkim.Evaluator.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Rules.Record
{
    [TestFixture]
    public class SelectorShouldBeWellConfiguredTests
    {
        private SelectorShouldBeWellConfigured _selectorShouldBeWellConfigured;
        private IRsaPublicKeyEvaluator _rsaPublicKeyEvaluator;

        [SetUp]
        public void SetUp()
        {
            _rsaPublicKeyEvaluator = A.Fake<IRsaPublicKeyEvaluator>();
            _selectorShouldBeWellConfigured = new SelectorShouldBeWellConfigured(_rsaPublicKeyEvaluator);
        }

        [Test]
        public async Task EvaluateEmptyPublicKeyProducesCorrectMessage()
        {
            PublicKeyData publicKeyData = new PublicKeyData("");
            PublicKeyType publicKeyType = new PublicKeyType("", KeyType.Rsa);
            string selector = "selector1";

            DkimRecord dkimRecord = new DkimRecord(null, new List<Tag> {publicKeyData, publicKeyType});

            List<EvaluationError> result =
                await _selectorShouldBeWellConfigured.Evaluate(new DkimEvaluationObject(selector, dkimRecord, null));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Guid.Parse("7150fec3-0464-4cd5-b774-d1d7bcff7ec5"), result[0].Id);
        }
    }
}
