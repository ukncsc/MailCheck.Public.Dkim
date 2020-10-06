using System.Collections.Generic;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class FlagTypeImplicitProvider : ImplicitTagProviderStrategyBase<Flags>
    {
        public FlagTypeImplicitProvider() : base(t => new Flags(string.Empty, new List<FlagTypeValue>(), true)){}
    }
}