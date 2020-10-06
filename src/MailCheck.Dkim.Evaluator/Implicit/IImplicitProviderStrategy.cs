using System.Collections.Generic;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public interface IImplicitProviderStrategy<T>
    {
        bool TryGetImplicitTag(List<T> ts, out T t);
    }
}