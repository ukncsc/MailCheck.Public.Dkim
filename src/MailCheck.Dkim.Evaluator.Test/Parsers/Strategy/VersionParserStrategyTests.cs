using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class VersionParserStrategyTests
    {
        [TestCase("DKIM1", null, null, TestName = "DKIM1 is valid value")]
        [TestCase("dkim1", "Selector test-selector. Invalid value 'dkim1' for v", EvaluationErrorType.Error, TestName = "lower case is invalid")]
        [TestCase("DKIM", "Selector test-selector. Invalid value 'DKIM' for v", EvaluationErrorType.Error, TestName = "DKIM is invalid value")]
        [TestCase(null, "Selector test-selector. Invalid value 'null' for v", EvaluationErrorType.Error, TestName = "null is invalid value")]
        [TestCase("", "Selector test-selector. Invalid value '' for v", EvaluationErrorType.Error, TestName = "empty string is invalid value")]
        public void VersionParserStategy(string tokenValue, string errorMessageStartsWith, EvaluationErrorType? type)
        {
            VersionParserStrategy versionParserStrategy = new VersionParserStrategy();

            EvaluationResult<Tag> tag = versionParserStrategy.Parse("test-selector", tokenValue);

            if (errorMessageStartsWith == null)
            {
                Assert.That(tag.Errors, Is.Empty);
            }
            else
            {
                Assert.That(tag.Errors.Count, Is.EqualTo(1));
                Assert.That(tag.Errors[0].Message.StartsWith(errorMessageStartsWith), Is.EqualTo(true));
                Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(type));
            }
        }
    }
}
