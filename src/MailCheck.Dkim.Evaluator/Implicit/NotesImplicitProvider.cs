using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class NotesImplicitProvider : ImplicitTagProviderStrategyBase<Notes>
    {
        public NotesImplicitProvider() : base(t => new Notes(string.Empty, true)){}
    }
}