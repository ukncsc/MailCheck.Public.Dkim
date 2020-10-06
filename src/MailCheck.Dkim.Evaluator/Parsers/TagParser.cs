using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers
{
    public interface ITagParser
    {
        EvaluationResult<List<Tag>> Parse(List<string> stringTags);
    }

    public class TagParser : ITagParser
    {
        private const char Separator = '=';
        private readonly Dictionary<string, ITagParserStrategy> _strategies;

        public TagParser(IEnumerable<ITagParserStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(_ => _.Tag);
        }

        public EvaluationResult<List<Tag>> Parse(List<string> stringTags)
        {
            List<EvaluationResult<List<Tag>>> results = stringTags
                .Select(_ => _.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(_ => _[0].Trim().ToLower())
                .ToDictionary(_ => _.Key, _ => _.Select(t => t.ElementAtOrDefault(1) ?? string.Empty).ToArray())
                .Select(_ => Parse(_.Key, _.Value))
                .ToList();

            return new EvaluationResult<List<Tag>>(
                results.SelectMany(_ => _.Item).ToList(),
                results.SelectMany(_ => _.Errors).ToList());
        }

        private EvaluationResult<List<Tag>> Parse(string tag, string[] values)
        {
            if (_strategies.TryGetValue(tag, out ITagParserStrategy strategy))
            {
                List<EvaluationResult<Tag>> results = values.Select(strategy.Parse).ToList();

                List<Tag> tags = results.Select(_ => _.Item).ToList();
                List<EvaluationError> errors = results.SelectMany(_ => _.Errors).ToList();

                if (values.Length > strategy.MaxOccurences)
                {
                    Guid Error1Id = Guid.Parse("230DAD53-3637-4853-BC8D-6A5798D9F543");

                    errors.Add(new EvaluationError(Error1Id, EvaluationErrorType.Error,
                        string.Format(DKimEvaluatorParsersResources.MoreThanMaxOccursErrorMessage, strategy.Tag, GetOccurrences(strategy.MaxOccurences), values.Length),
                        string.Format(DKimEvaluatorParsersMarkdownResources.MoreThanMaxOccursErrorMessage, strategy.Tag, GetOccurrences(strategy.MaxOccurences), values.Length)));
                }
                return new EvaluationResult<List<Tag>>(tags, errors);
            }

            List<Tag> unknownTags = values.Select(_ => new UnknownTag(tag, _)).Cast<Tag>().ToList();

            Guid Error2Id = Guid.Parse("7B14FB5A-C47E-4E35-A328-8FC13B9A8CA0");

            List<EvaluationError> unknownTagErrors = values.Select(_ => new EvaluationError(Error2Id, EvaluationErrorType.Error,
                string.Format(DKimEvaluatorParsersResources.UnknownTagErrorMessage, tag, _ ?? "<null>"),
                string.Format(DKimEvaluatorParsersMarkdownResources.UnknownTagErrorMessage, tag, _ ?? "<null>"))).ToList();

            return new EvaluationResult<List<Tag>>(unknownTags, unknownTagErrors);
        }
        
        public string GetOccurrences(int occurrences)
        {
            return occurrences == 1 ? "once" : (occurrences == 2 ? "twice" : $"{occurrences} times");
        }
    }
}
