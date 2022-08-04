using System;
using System.Text.RegularExpressions;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class PublicKeyDataParserStrategy : ITagParserStrategy
    {
        private readonly Regex _fwsRegex = new Regex("(?<!\\r\\n)[\\x20\\t]+|\\r\\n[\\x20\\t]+");
        private readonly Regex _base64Regex = new Regex("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");

        public Guid Id = Guid.Parse("9E215A23-4C1B-4BD1-BCF4-FB99788D9C95");

        public EvaluationResult<Tag> Parse(string selector, string value)
        {
            //first remove all legal folding white space
            string trimmedValues = _fwsRegex.Replace(value ?? string.Empty, string.Empty);

            //then check if the result if a valid base 64 string
            PublicKeyData publicKeyData = new PublicKeyData(value);

            if (!_base64Regex.IsMatch(trimmedValues))
            {
                EvaluationError error = new EvaluationError(Id, "mailcheck.dkim.invalidPublicKey", EvaluationErrorType.Error,
                    string.Format(DKimEvaluatorParsersResources.InvalidPublicKeyErrorMessage, selector, value ?? "null"),
                    string.Format(DKimEvaluatorParsersMarkdownResources.InvalidPublicKeyErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(publicKeyData, error);
            }

            return new EvaluationResult<Tag>(publicKeyData);
        }

        public string Tag => "p";
        public int MaxOccurences => 1;
    }
}
