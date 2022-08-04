using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;
using MailCheck.Dkim.Evaluator.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Rules.Record
{
    [TestFixture]
    public class TagShouldHavePublicKeyDataTests
    {
        private TagShouldHavePublicKeyData _tagShouldHavePublicKeyData;

        [SetUp]
        public void SetUp()
        {
            _tagShouldHavePublicKeyData = new TagShouldHavePublicKeyData();
        }

        [Test]
        public async Task EvaluateNoPublicKeyDataProducesErrorMessage()
        {
            string selector = "selector1";

            DkimRecord dkimRecord = new DkimRecord(null, new List<Tag>());

            List<EvaluationError> result =
                await _tagShouldHavePublicKeyData.Evaluate(new DkimEvaluationObject(selector, dkimRecord, null));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(EvaluationErrorType.Error, result[0].ErrorType);
        }

        [Test]
        public async Task EvaluateNoPublicKeyDataWithAmazonSESProducesInfoMessage()
        {
            string selector = "selector1";

            DkimRecord dkimRecord = new DkimRecord(null, new List<Tag>());

            List<EvaluationError> result =
                await _tagShouldHavePublicKeyData.Evaluate(new DkimEvaluationObject(selector, dkimRecord, "selector1.dkim.amazonses.com."));

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(EvaluationErrorType.Info, result[0].ErrorType);
        }

        [Test]
        public async Task EvaluatePublicKeyDataExistsProducesNoErrors()
        {
            PublicKeyData publicKeyData = new PublicKeyData("");
            PublicKeyType publicKeyType = new PublicKeyType("", KeyType.Rsa);
            string selector = "selector1";

            DkimRecord dkimRecord = new DkimRecord(null, new List<Tag> { publicKeyData, publicKeyType });

            List<EvaluationError> result =
                await _tagShouldHavePublicKeyData.Evaluate(new DkimEvaluationObject(selector, dkimRecord, null));

            Assert.AreEqual(0, result.Count);
        }
    }
}
