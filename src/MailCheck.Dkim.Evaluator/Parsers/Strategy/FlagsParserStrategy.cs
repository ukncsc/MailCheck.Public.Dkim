using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class FlagsParserStrategy : ITagParserStrategy
    {
        private const string Separator = ":";

        public EvaluationResult<Tag> Parse(string value)
        {
            string[] tokens = value?.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Trim()).ToArray();

            if (tokens == null || !tokens.Any())
            {
                Guid Error1Id = Guid.Parse("F59F5073-FDD3-4400-A5A9-62DCDC6B0C3B");

                EvaluationError error = new EvaluationError(Error1Id, EvaluationErrorType.Error,
                    string.Format(DKimEvaluatorParsersResources.InvalidFlagValueErrorMessage, value ?? "null"),
                    string.Format(DKimEvaluatorParsersMarkdownResources.InvalidFlagValueErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(new Flags(value, new List<FlagTypeValue>()), error);
            }

            List<FlagTypeValue> flagTypeValues = tokens.Select(GetFlagTypeValue).ToList();

            Guid Error2Id = Guid.Parse("550293DB-D88F-40BA-9EF9-6CF28968610D");

            List<EvaluationError> errors = flagTypeValues.Where(_ => _.Type == FlagType.Unknown)
                .Select(_ => new EvaluationError(Error2Id, EvaluationErrorType.Error,
                string.Format(DKimEvaluatorParsersResources.InvalidFalgTypeErrorMessage, _.Value ?? "null"),
                string.Format(DKimEvaluatorParsersMarkdownResources.InvalidFalgTypeErrorMessage, _.Value ?? "null")))
                .ToList();

            return new EvaluationResult<Tag>(new Flags(value, flagTypeValues), errors);
        }

        public FlagTypeValue GetFlagTypeValue(string value)
        {
            switch (value)
            {
                case "y":
                    return new FlagTypeValue(value, FlagType.Y);
                case "s":
                    return new FlagTypeValue(value, FlagType.S);
                default:
                    return new FlagTypeValue(value, FlagType.Unknown);
            }
        }

        public string Tag => "t";
        public int MaxOccurences => 1;
    }
}
