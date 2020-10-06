using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class PublicKeyTypeParserStrategyTests
    {
        [TestCase(null, KeyType.Unknown, "Invalid value 'null' for k", EvaluationErrorType.Error, TestName = "null is invalid value")]
        [TestCase("", KeyType.Unknown, "Invalid value '' for k", EvaluationErrorType.Error, TestName = "empty string is invalid value")]
        [TestCase("RSA", KeyType.Unknown, "Invalid value 'RSA' for k", EvaluationErrorType.Error, TestName = "upper case RSA invalid")]
        [TestCase("rsa", KeyType.Rsa, null, null, TestName = "rsa valid")]
        [TestCase("ED25519", KeyType.Unknown, "Invalid value 'ED25519' for k", EvaluationErrorType.Error, TestName = "upper case ED25519 invalid")]
        [TestCase("ed25519", KeyType.Ed25519, null, null, TestName = "ed25519 valid")]
        public void PublicKeyTypeParserStrategy(string tokenValue, KeyType keyType, string errorMessageStartsWith, EvaluationErrorType? type)
        {
            PublicKeyTypeParserStrategy publicKeyTypeParserStrategy = new PublicKeyTypeParserStrategy();
            EvaluationResult<Tag> publicKeyTypeResult = publicKeyTypeParserStrategy.Parse(tokenValue);

            PublicKeyType publicKeyType = (PublicKeyType)publicKeyTypeResult.Item;

            if (errorMessageStartsWith == null)
            {
                Assert.That(publicKeyTypeResult.Errors, Is.Empty);
                Assert.That(publicKeyType.Value, Is.EqualTo(tokenValue));
                Assert.That(publicKeyType.KeyType, Is.EqualTo(keyType));
            }
            else
            {
                Assert.That(publicKeyTypeResult.Errors.Count, Is.EqualTo(1));
                Assert.That(publicKeyTypeResult.Errors[0].Message.StartsWith(errorMessageStartsWith), Is.EqualTo(true));
                Assert.That(publicKeyTypeResult.Errors[0].ErrorType, Is.EqualTo(type));
                Assert.That(publicKeyType.Value, Is.EqualTo(tokenValue));
                Assert.That(publicKeyType.KeyType, Is.EqualTo(keyType));
            }
        }
    }
}
