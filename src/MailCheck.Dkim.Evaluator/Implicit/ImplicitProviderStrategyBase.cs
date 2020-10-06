using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public abstract class ImplicitTagProviderStrategyBase<TConcrete>
        : ImplicitProviderStrategyBase<Tag, TConcrete>
        where TConcrete : Tag
    {
        protected ImplicitTagProviderStrategyBase(Func<List<Tag>, TConcrete> defaultValueFactory)
            : base(defaultValueFactory)
        { }
    }

    public abstract class ImplicitProviderStrategyBase<TBase, TConcrete> : IImplicitProviderStrategy<TBase>
        where TConcrete : TBase
        where TBase : class
    {
        private readonly Func<List<TBase>, TConcrete> _defaultValueFactory;

        protected ImplicitProviderStrategyBase(Func<List<TBase>, TConcrete> defaultValueFactory)
        {
            _defaultValueFactory = defaultValueFactory;
        }

        public bool TryGetImplicitTag(List<TBase> ts, out TBase t)
        {
            if (ts.OfType<TConcrete>().Any())
            {
                t = null;
                return false;
            }

            t = _defaultValueFactory(ts);
            return true;
        }
    }
}