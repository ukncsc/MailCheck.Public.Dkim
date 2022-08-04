using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Rules.Record
{
    public class TagShouldHavePublicKeyData : IRule<DkimEvaluationObject>
    {
        public Guid ErrorId = Guid.Parse("F3895EF6-8709-499A-8C5C-40118A818087");
        public Guid InfoId = Guid.Parse("d65df796-89d6-41e9-a232-d1d4ea7eabb6");

        public Task<List<EvaluationError>> Evaluate(DkimEvaluationObject evaluationItem)
        {
            List<EvaluationError> errors = new List<EvaluationError>();

            if (!evaluationItem.Record.Tags.OfType<PublicKeyData>().Any())
            {
                if (evaluationItem.CName != null
                    && (evaluationItem.CName.EndsWith("amazonses.com.") || evaluationItem.CName.EndsWith("amazonses.com")))
                {
                    errors.Add(new EvaluationError(InfoId, "mailcheck.dkim.selectorNotInUse", EvaluationErrorType.Info,
                        string.Format(DKimEvaluatorRulesResources.SelectorNotInUseInfoMessage, evaluationItem.Selector),
                        string.Empty));
                }
                else
                {
                    errors.Add(new EvaluationError(ErrorId, "mailcheck.dkim.publicKeyDataTagMustExist", EvaluationErrorType.Error,
                        string.Format(DKimEvaluatorRulesResources.PublicKeyDataTagMustExistErrorMessage, evaluationItem.Selector),
                        DKimEvaluatorRulesMarkdownResources.PublicKeyDataTagMustExistErrorMessage));
                }
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;

        public bool IsStopRule => true;
    }
}