using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class ServiceTypeParserStrategyTests
    {
        [TestCase(null, ServiceTypeType.Unknown, "Selector test-selector. Invalid value 'null' for s", EvaluationErrorType.Error, TestName = "null is invalid value")]
        [TestCase("", ServiceTypeType.Unknown, "Selector test-selector. Invalid value '' for s", EvaluationErrorType.Error, TestName = "empty string is invalid value")]
        [TestCase("email", ServiceTypeType.Email, null, null, TestName = "email is valid value")]
        [TestCase("EmAiL", ServiceTypeType.Unknown, "Selector test-selector. Invalid value 'EmAiL' for s", EvaluationErrorType.Error, TestName = "non lowercase EmAiL is invalid value")]
        [TestCase("*", ServiceTypeType.Any, null, null, TestName = "* is valid value")]
        [TestCase("test", ServiceTypeType.Unknown, "Selector test-selector. Invalid value 'test' for s", EvaluationErrorType.Error, TestName = "random string is invalid value")]
        public void ServiceTypeParserStrategy(string tokenValue, ServiceTypeType serviceTypeType, string errorMessageStartsWith, EvaluationErrorType? errorType)
        {
            ServiceTypeParserStrategy serviceTypeParserStrategy = new ServiceTypeParserStrategy();

            EvaluationResult<Tag> serviceTypeResult = serviceTypeParserStrategy.Parse("test-selector", tokenValue);

            ServiceType serviceType = (ServiceType)serviceTypeResult.Item;

            if (errorMessageStartsWith == null)
            {
                Assert.That(serviceTypeResult.Errors, Is.Empty);
                Assert.That(serviceType.Value, Is.EqualTo(tokenValue));
                Assert.That(serviceType.Type, Is.EqualTo(serviceTypeType));
            }
            else
            {
                Assert.That(serviceTypeResult.Errors.Count, Is.EqualTo(1));
                Assert.That(serviceTypeResult.Errors[0].Message.StartsWith(errorMessageStartsWith), Is.EqualTo(true));
                Assert.That(serviceTypeResult.Errors[0].ErrorType, Is.EqualTo(errorType));
                Assert.That(serviceType.Value, Is.EqualTo(tokenValue));
                Assert.That(serviceType.Type, Is.EqualTo(serviceTypeType));
            }
        }
    }
}
