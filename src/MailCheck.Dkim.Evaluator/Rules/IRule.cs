using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailCheck.Dkim.Evaluator.Rules
{
    public interface IRule<in T>
    {
        Task<List<EvaluationError>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}
