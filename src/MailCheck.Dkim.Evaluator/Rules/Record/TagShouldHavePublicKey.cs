using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Rules.Record
{
    public class TagShouldHavePublicKeyData : IRule<DkimRecord>
    {
        public Guid Id = Guid.Parse("F3895EF6-8709-499A-8C5C-40118A818087");

        public Task<List<EvaluationError>> Evaluate(DkimRecord record)
        {
            List<EvaluationError> errors = new List<EvaluationError>();

            if (!record.Tags.OfType<PublicKeyData>().Any())
            {
                errors.Add(new EvaluationError(Id, EvaluationErrorType.Error,
                    DKimEvaluatorRulesResources.PublicKeyDataTagMustExistErrorMessage,
                    DKimEvaluatorRulesMarkdownResources.PublicKeyDataTagMustExistErrorMessage));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;

        public bool IsStopRule => true;
    }
}