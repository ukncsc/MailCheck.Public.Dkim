using System;
using System.Text.RegularExpressions;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;
using Version = MailCheck.Dkim.Evaluator.Domain.Version;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class VersionParserStrategy : ITagParserStrategy
    {
        private readonly Regex _regex = new Regex("DKIM1$");

        public Guid Id = Guid.Parse("ECE582A3-79AB-48DB-8028-7AAE18383E0E");

        public EvaluationResult<Tag> Parse(string value)
        {
            Version version = new Version(value);

            if (!_regex.IsMatch(value ?? string.Empty))
            {
                EvaluationError error = new EvaluationError(Id, EvaluationErrorType.Error,
                   string.Format(DKimEvaluatorParsersResources.InvalidVersionErrorMessage, value ?? "null"),
                   string.Format(DKimEvaluatorParsersMarkdownResources.InvalidVersionErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(version, error);
            }

            return new EvaluationResult<Tag>(version);
        }

        public string Tag => "v";
        public int MaxOccurences => 1;
    }
}
