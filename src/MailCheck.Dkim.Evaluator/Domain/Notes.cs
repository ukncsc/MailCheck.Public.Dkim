namespace MailCheck.Dkim.Evaluator.Domain
{
    public class Notes : Tag
    {
        public Notes(string value, bool isImplicit = false) 
            : base(value, isImplicit)
        {
        }
    }
}
