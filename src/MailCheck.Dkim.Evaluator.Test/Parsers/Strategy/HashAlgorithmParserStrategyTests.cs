using System;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class HashAlgorithmParserStrategyTests
    {
        private HashAlgorithmParserStrategy _hashAlgorithmParserStrategy;

        [SetUp]
        public void SetUp()
        {
            _hashAlgorithmParserStrategy = new HashAlgorithmParserStrategy();
        }

        [Test]
        public void NullValueCausesError()
        {
            EvaluationResult<Tag> tag = _hashAlgorithmParserStrategy.Parse("test-selector", null);

            Assert.That(tag.Item, Is.TypeOf<HashAlgorithm>());

            HashAlgorithm hashAlgorithm = (HashAlgorithm)tag.Item;

            Assert.That(hashAlgorithm.Value, Is.Null);
            Assert.That(hashAlgorithm.HashAlgorithms, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Selector test-selector. Invalid value 'null' for h"), Is.True);
        }

        [Test]
        public void EmptyStringCausesError()
        {
            EvaluationResult<Tag> tag = _hashAlgorithmParserStrategy.Parse("test-selector", string.Empty);

            Assert.That(tag.Item, Is.TypeOf<HashAlgorithm>());

            HashAlgorithm hashAlgorithm = (HashAlgorithm)tag.Item;

            Assert.That(hashAlgorithm.Value, Is.Empty);
            Assert.That(hashAlgorithm.HashAlgorithms, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Selector test-selector. Invalid value '' for h"), Is.True);
        }

        [Test]
        public void ColonCausesError()
        {
            EvaluationResult<Tag> tag = _hashAlgorithmParserStrategy.Parse("test-selector", ":");

            Assert.That(tag.Item, Is.TypeOf<HashAlgorithm>());

            HashAlgorithm hashAlgorithm = (HashAlgorithm)tag.Item;

            Assert.That(hashAlgorithm.Value, Is.EqualTo(":"));
            Assert.That(hashAlgorithm.HashAlgorithms, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Selector test-selector. Invalid value ':' for h"), Is.True);
        }

        [Test]
        public void InvalidAlgorithmValueCausesError()
        {
            EvaluationResult<Tag> tag = _hashAlgorithmParserStrategy.Parse("test-selector", "test:sha1");

            Assert.That(tag.Item, Is.TypeOf<HashAlgorithm>());

            HashAlgorithm hashAlgorithm = (HashAlgorithm)tag.Item;

            Assert.That(hashAlgorithm.Value, Is.EqualTo("test:sha1"));
            Assert.That(hashAlgorithm.HashAlgorithms.Count, Is.EqualTo(2));
            Assert.That(hashAlgorithm.HashAlgorithms[0].Value, Is.EqualTo("test"));
            Assert.That(hashAlgorithm.HashAlgorithms[0].Type, Is.EqualTo(HashAlgorithmType.Unknown));
            Assert.That(hashAlgorithm.HashAlgorithms[1].Value, Is.EqualTo("sha1"));
            Assert.That(hashAlgorithm.HashAlgorithms[1].Type, Is.EqualTo(HashAlgorithmType.Sha1));
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Selector test-selector. Invalid value 'test' for hash algorithm"), Is.True);
        }

        [Test]
        public void ValidAlgorithmsNoErrors()
        {
            EvaluationResult<Tag> tag = _hashAlgorithmParserStrategy.Parse(string.Empty, "sha256");

            Assert.That(tag.Item, Is.TypeOf<HashAlgorithm>());

            HashAlgorithm hashAlgorithm = (HashAlgorithm)tag.Item;

            Assert.That(hashAlgorithm.Value, Is.EqualTo("sha256"));
            Assert.That(hashAlgorithm.HashAlgorithms.Count, Is.EqualTo(1));
            Assert.That(hashAlgorithm.HashAlgorithms[0].Value, Is.EqualTo("sha256"));
            Assert.That(hashAlgorithm.HashAlgorithms[0].Type, Is.EqualTo(HashAlgorithmType.Sha256));
            Assert.That(tag.Errors, Is.Empty);
        }
    }
}
