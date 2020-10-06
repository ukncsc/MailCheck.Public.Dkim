using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.DomainStatus.Contracts;

namespace MailCheck.Dkim.Entity.Entity.DomainStatus
{
    public interface IDomainStatusEvaluator
    {
        Status GetStatus(List<DkimEvaluatorMessage> evaluatorMessages, IEnumerable<Contracts.SharedDomain.Message> messages);
    }

    public class DomainStatusEvaluator : IDomainStatusEvaluator
    {
        public Status GetStatus(List<DkimEvaluatorMessage> evaluatorMessages, IEnumerable<Contracts.SharedDomain.Message> messages)
        {
            List<EvaluationErrorType> evaluationErrors = evaluatorMessages?.Select(x => x.ErrorType).ToList();
            List<Contracts.SharedDomain.MessageType> messageTypes = messages?.Select(x => x.MessageType).ToList();
            Status status = Status.Success;

            if (messageTypes != null && messageTypes.Any(x => x == Contracts.SharedDomain.MessageType.error) ||
                evaluationErrors != null && evaluationErrors.Any(x => x == EvaluationErrorType.Error))
            {
                status = Status.Error;
            }
            else if (messageTypes != null && messageTypes.Any(x => x == Contracts.SharedDomain.MessageType.warning) ||
                     evaluationErrors != null && evaluationErrors.Any(x => x == EvaluationErrorType.Warning))
            {
                status = Status.Warning;
            }
            else if (messageTypes != null && messageTypes.Any(x => x == Contracts.SharedDomain.MessageType.info) ||
                evaluationErrors != null && evaluationErrors.Any(x => x == EvaluationErrorType.Info))
            {
                status = Status.Info;
            }

            return status;
        }
    }
}
