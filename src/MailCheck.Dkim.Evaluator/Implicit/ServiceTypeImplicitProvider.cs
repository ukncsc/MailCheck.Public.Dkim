using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class ServiceTypeImplicitProvider : ImplicitTagProviderStrategyBase<ServiceType>
    {
        public ServiceTypeImplicitProvider() : base(t => new ServiceType("*", ServiceTypeType.Any, true)){}
    }
}