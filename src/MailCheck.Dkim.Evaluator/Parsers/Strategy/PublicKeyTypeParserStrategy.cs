using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;
using System;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class PublicKeyTypeParserStrategy : ITagParserStrategy
    {
        public Guid Id = Guid.Parse("3FF895EA-1622-4D8C-82C5-0642399F2937");

        public EvaluationResult<Tag> Parse(string selector, string value)
        {
            KeyType keyType = GetPublicKeyType(value);

            PublicKeyType publicKeyType = new PublicKeyType(value, keyType);

            if (keyType == KeyType.Unknown)
            {
                EvaluationError error = new EvaluationError(Id, "mailcheck.dkim.invalidPublicKeyType", EvaluationErrorType.Error,
                    string.Format(DKimEvaluatorParsersResources.InvalidPublicTypeErrorMessage, selector, value ?? "null"),
                    string.Format(DKimEvaluatorParsersMarkdownResources.InvalidPublicTypeErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(publicKeyType, error);
            }

            return new EvaluationResult<Tag>(publicKeyType);
        }

        private KeyType GetPublicKeyType(string value)
        {
            switch (value)
            {
                case "rsa":
                    return KeyType.Rsa;
                case "ed25519":
                    return KeyType.Ed25519;
                default:
                    return KeyType.Unknown;
            }
        }

        public string Tag => "k";
        public int MaxOccurences => 1;
    }
}
