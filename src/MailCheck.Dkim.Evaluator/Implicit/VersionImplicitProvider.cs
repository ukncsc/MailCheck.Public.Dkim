using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class VersionImplicitProvider : ImplicitTagProviderStrategyBase<Version>
    {
        public VersionImplicitProvider() : base(t => new Version("VDKIM1", true)){}
    }
}