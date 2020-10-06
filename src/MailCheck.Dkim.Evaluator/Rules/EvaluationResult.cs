using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Dkim.Evaluator.Rules
{
    public class EvaluationResult<T>
    {
        public EvaluationResult(T item, params EvaluationError[] errors)
        {
            Item = item;
            Errors = errors.ToList();
        }

        public EvaluationResult(T item, List<EvaluationError> errors)
        {
            Item = item;
            Errors = errors;
        }

        public T Item { get; }

        public List<EvaluationError> Errors { get; }
    }
}