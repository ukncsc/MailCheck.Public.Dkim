namespace MailCheck.Dkim.Evaluator.Domain
{
    public class PublicKeyType : Tag
    {
        public PublicKeyType(string value, KeyType keyType, bool isImplicit = false) 
            : base(value, isImplicit)
        {
            KeyType = keyType;
        }

        public KeyType KeyType { get; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(KeyType)}: {KeyType}";
        }
    }
}
