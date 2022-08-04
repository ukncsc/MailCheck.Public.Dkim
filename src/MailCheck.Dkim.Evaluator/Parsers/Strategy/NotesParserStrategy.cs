using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class NotesParserStrategy : ITagParserStrategy
    {
        public EvaluationResult<Tag> Parse(string selector, string value)
        {
            string note = value != null
                ? string.Format(DKimEvaluatorParsersResources.NoteMessage, selector, value)
                : string.Empty;

            return new EvaluationResult<Tag>(new Notes(note));
        }

        public string Tag => "n";
        public int MaxOccurences => 1;
    }
}
