using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class NotesParserStrategy : ITagParserStrategy
    {
        public EvaluationResult<Tag> Parse(string value)
        {
            return new EvaluationResult<Tag>(new Notes(value ?? string.Empty));
        }

        public string Tag => "n";
        public int MaxOccurences => 1;
    }
}
