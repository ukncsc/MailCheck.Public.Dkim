namespace MailCheck.Dkim.Evaluator.Domain
{
    public class FlagTypeValue
    {
        public FlagTypeValue(string value, FlagType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; }
        public FlagType Type { get; }
    }
}