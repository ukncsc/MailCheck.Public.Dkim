namespace MailCheck.Dkim.Evaluator.Rules
{
    public class DkimEvaluationObject
    {
        public DkimEvaluationObject(string selector, Domain.DkimRecord record, string cname)
        {
            Selector = selector;
            Record = record;
            CName = cname;
        }

        public string Selector { get; }
        public Domain.DkimRecord Record { get; }
        public string CName { get; }
    }
}
