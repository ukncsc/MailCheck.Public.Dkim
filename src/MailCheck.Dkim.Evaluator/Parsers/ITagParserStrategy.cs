using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers
{
    public interface ITagParserStrategy
    {
        EvaluationResult<Tag> Parse(string selector, string value);
        string Tag { get; }
        int MaxOccurences { get; }
    }
}
