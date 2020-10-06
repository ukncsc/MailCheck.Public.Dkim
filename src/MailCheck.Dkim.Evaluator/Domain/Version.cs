namespace MailCheck.Dkim.Evaluator.Domain
{
    public class Version : Tag
    {
        public Version(string value, bool isImplicit = false) :
            base(value, isImplicit)
        {
        }
    }
}
