namespace MailCheck.Dkim.Evaluator.Domain
{
    public class UnknownTag : Tag
    {
        public UnknownTag(string type, string value)
            : base(value)
        {
            Type = type;
        }

        public string Type { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Type)}: {Type}";
        }
    }
}
