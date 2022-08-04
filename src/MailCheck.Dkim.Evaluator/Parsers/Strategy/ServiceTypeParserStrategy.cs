using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;
using System;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class ServiceTypeParserStrategy : ITagParserStrategy
    {
        public Guid Id = Guid.Parse("6DCF2BE4-50C8-4C84-B659-5B50B6E46DE8");

        public EvaluationResult<Tag> Parse(string selector, string value)
        {
            ServiceTypeType serviceTypeType = GetServiceTypeType(value);

            ServiceType serviceType = new ServiceType(value, serviceTypeType);

            if (serviceTypeType == ServiceTypeType.Unknown)
            {
                EvaluationError error = new EvaluationError(Id, "mailcheck.dkim.invalidServiceType", EvaluationErrorType.Error,
                    string.Format(DKimEvaluatorParsersResources.InvalidServiceTypeErrorMessage, selector, value ?? "null"),
                    string.Format(DKimEvaluatorParsersMarkdownResources.InvalidServiceTypeErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(serviceType, error);
            }

            return new EvaluationResult<Tag>(serviceType);
        }

        private ServiceTypeType GetServiceTypeType(string value)
        {
            switch (value)
            {
                case "*":
                    return ServiceTypeType.Any;
                case "email":
                    return ServiceTypeType.Email;
                default:
                    return ServiceTypeType.Unknown;
            }
        }

        public string Tag => "s";
        public int MaxOccurences => 1;
    }
}
