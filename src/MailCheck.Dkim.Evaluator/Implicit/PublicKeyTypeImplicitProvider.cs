using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class PublicKeyTypeImplicitProvider : ImplicitTagProviderStrategyBase<PublicKeyType>
    {
        public PublicKeyTypeImplicitProvider() : base(t => new PublicKeyType("rsa", KeyType.Rsa, true)){}
    }
}