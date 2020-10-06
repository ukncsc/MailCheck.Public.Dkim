using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;

namespace MailCheck.Dkim.Evaluator.Parsers.Strategy
{
    public class HashAlgorithmParserStrategy : ITagParserStrategy
    {
        private const string Separator = ":";

        public EvaluationResult<Tag> Parse(string value)
        {
            string[] tokens = value?.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Trim()).ToArray();

            if (tokens == null || !tokens.Any())
            {
                Guid Error1Id = Guid.Parse("2028085F-0112-48CD-8E84-5E9E81B18D04");

                EvaluationError error = new EvaluationError(Error1Id, EvaluationErrorType.Error,
                            string.Format(DKimEvaluatorParsersResources.InvalidHashAlgorithmErrorMessage, value ?? "null"),
                            string.Format(DKimEvaluatorParsersMarkdownResources.InvalidHashAlgorithmErrorMessage, value ?? "null"));

                return new EvaluationResult<Tag>(new HashAlgorithm(value, new List<HashAlgorithmValue>()), error);
            }

            List<HashAlgorithmValue> hashAlgorithmValues = tokens.Select(GetHashAlgorithmValue).ToList();

            Guid Error2Id = Guid.Parse("8BC9C8EF-174D-415C-B6D1-2BA5C57C63A9");

            List<EvaluationError> errors = hashAlgorithmValues.Where(_ => _.Type == HashAlgorithmType.Unknown)
                .Select(_ => new EvaluationError(Error2Id, EvaluationErrorType.Error, string.Format(DKimEvaluatorParsersResources.UnknownHashAlgorithmErrorMessage, _.Value ?? "null"),
                string.Format(DKimEvaluatorParsersMarkdownResources.UnknownHashAlgorithmErrorMessage, _.Value ?? "null"))).ToList();

            return new EvaluationResult<Tag>(new HashAlgorithm(value, hashAlgorithmValues), errors);
        }

        private HashAlgorithmValue GetHashAlgorithmValue(string value)
        {
            switch (value)
            {
                case "sha1":
                    return new HashAlgorithmValue(value, HashAlgorithmType.Sha1);
                case "sha256":
                    return new HashAlgorithmValue(value, HashAlgorithmType.Sha256);
                default:
                    return new HashAlgorithmValue(value, HashAlgorithmType.Unknown);
            }
        }

        public string Tag => "h";
        public int MaxOccurences => 1;
    }
}
