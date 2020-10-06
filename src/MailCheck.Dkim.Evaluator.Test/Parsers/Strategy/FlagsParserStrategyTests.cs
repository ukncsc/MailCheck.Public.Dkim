using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class FlagsParserStrategyTests
    {
        private FlagsParserStrategy _flagsParserStrategy;

        [SetUp]
        public void SetUp()
        {
            _flagsParserStrategy = new FlagsParserStrategy();
        }

        [Test]
        public void NullValueInvalid()
        {
            EvaluationResult<Tag> tag = _flagsParserStrategy.Parse(null);

            Assert.That(tag.Item, Is.TypeOf<Flags>());

            Flags flags = (Flags)tag.Item;

            Assert.That(flags.Value, Is.Null);
            Assert.That(flags.FlagTypes, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Invalid value 'null' for t"), Is.True);
        }

        [Test]
        public void EmptyStringInvalid()
        {
            EvaluationResult<Tag> tag = _flagsParserStrategy.Parse(string.Empty);

            Assert.That(tag.Item, Is.TypeOf<Flags>());

            Flags flags = (Flags)tag.Item;

            Assert.That(flags.Value, Is.EqualTo(string.Empty));
            Assert.That(flags.FlagTypes, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Invalid value '' for t"), Is.True);
        }

        [Test]
        public void ColonInvalid()
        {
            EvaluationResult<Tag> tag = _flagsParserStrategy.Parse(":");

            Assert.That(tag.Item, Is.TypeOf<Flags>());

            Flags flags = (Flags)tag.Item;

            Assert.That(flags.Value, Is.EqualTo(":"));
            Assert.That(flags.FlagTypes, Is.Empty);
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Invalid value ':' for t"), Is.True);
        }

        [Test]
        public void IncorrectFlagValueInvalid()
        {
            EvaluationResult<Tag> tag = _flagsParserStrategy.Parse("test:y");

            Assert.That(tag.Item, Is.TypeOf<Flags>());

            Flags flags = (Flags)tag.Item;

            Assert.That(flags.Value, Is.EqualTo("test:y"));
            Assert.That(flags.FlagTypes.Count, Is.EqualTo(2));
            Assert.That(flags.FlagTypes[0].Value, Is.EqualTo("test"));
            Assert.That(flags.FlagTypes[0].Type, Is.EqualTo(FlagType.Unknown));
            Assert.That(flags.FlagTypes[1].Value, Is.EqualTo("y"));
            Assert.That(flags.FlagTypes[1].Type, Is.EqualTo(FlagType.Y));
            Assert.That(tag.Errors.Count, Is.EqualTo(1));
            Assert.That(tag.Errors[0].ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(tag.Errors[0].Message.StartsWith("Invalid value 'test' for flag type"), Is.True);
        }

        [Test]
        public void CorrectFlagValueValid()
        {
            EvaluationResult<Tag> tag = _flagsParserStrategy.Parse("s");

            Assert.That(tag.Item, Is.TypeOf<Flags>());

            Flags flags = (Flags)tag.Item;

            Assert.That(flags.Value, Is.EqualTo("s"));
            Assert.That(flags.FlagTypes.Count, Is.EqualTo(1));
            Assert.That(flags.FlagTypes[0].Value, Is.EqualTo("s"));
            Assert.That(flags.FlagTypes[0].Type, Is.EqualTo(FlagType.S));
            Assert.That(tag.Errors, Is.Empty);
        }
    }
}
