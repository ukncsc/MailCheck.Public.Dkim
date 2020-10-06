namespace MailCheck.Dkim.Evaluator.Domain
{
    public class HashAlgorithmValue
    {
        public HashAlgorithmValue(string value, HashAlgorithmType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; }
        public HashAlgorithmType Type { get; }
    }
}