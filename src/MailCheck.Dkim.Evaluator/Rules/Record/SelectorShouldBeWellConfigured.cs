using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rsa;

namespace MailCheck.Dkim.Evaluator.Rules.Record
{
    public class SelectorShouldBeWellConfigured : IRule<DkimRecord>
    {
        private readonly IRsaPublicKeyEvaluator _publicKeyEvaluator;

        public SelectorShouldBeWellConfigured(IRsaPublicKeyEvaluator publicKeyEvaluator)
        {
            _publicKeyEvaluator = publicKeyEvaluator;
        }

        public Task<List<EvaluationError>> Evaluate(DkimRecord record)
        {
            PublicKeyData publicKeyData = record.Tags.OfType<PublicKeyData>().FirstOrDefault();
            PublicKeyType publicKeyType = record.Tags.OfType<PublicKeyType>().FirstOrDefault();

            List<EvaluationError> errors = new List<EvaluationError>();
            if (publicKeyData != null && publicKeyType != null && !string.IsNullOrEmpty(publicKeyData.Value))
            {
                if (publicKeyType.KeyType != KeyType.Unknown)
                {
                    switch (publicKeyType.KeyType)
                    {
                        case KeyType.Ed25519:
                            break;
                        case KeyType.Rsa:
                            string key = publicKeyData.Value.TrimEnd(';');


                            if (!_publicKeyEvaluator.TryGetKeyLengthSize(key, out int keyLength))
                            {
                                Guid Error3Id = Guid.Parse("21B09D43-685C-4CAE-989E-7194CA093863");

                                errors.Add(new EvaluationError(Error3Id, EvaluationErrorType.Error,
                                    DKimEvaluatorRulesResources.CorruptPublicKeyErrorMessage,
                                    DKimEvaluatorRulesMarkdownResources.CorruptPublicKeyErrorMessage));
                            }
                            else if (keyLength < 2048)
                            {
                                if (keyLength < 1024)
                                {
                                    Guid Error1Id = Guid.Parse("C740A1B0-394B-4397-A4E6-178C3137A84D");

                                    errors.Add(new EvaluationError(Error1Id, EvaluationErrorType.Error,
                                        string.Format(DKimEvaluatorRulesResources.PublicKeyIsToShortErrorMessage,
                                            keyLength),
                                       string.Empty));
                                }
                                else if (keyLength >= 1024 && keyLength < 2048)
                                {
                                    Guid Error2Id = Guid.Parse("D16BE8D5-8ED6-4DD0-A8AA-C8F1380A8D3C");

                                    errors.Add(new EvaluationError(Error2Id, EvaluationErrorType.Warning,
                                        string.Format(
                                            DKimEvaluatorRulesResources.PublicKeyIsToShortUseLongerErrorMessage,
                                            keyLength),
                                       string.Empty));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    Guid Error3Id = Guid.Parse("FF2380B4-8DE6-4B97-B69A-F3382EBF3DE2");

                    errors.Add(new EvaluationError(Error3Id, EvaluationErrorType.Error,
                        DKimEvaluatorRulesResources.IncorrectPublicKeyConfigured,
                        DKimEvaluatorRulesMarkdownResources.IncorrectPublicKeyConfigured));
                }
            }
            else
            {
                Guid Error3Id = Guid.Parse("7150FEC3-0464-4CD5-B774-D1D7BCFF7EC5");

                errors.Add(new EvaluationError(Error3Id, EvaluationErrorType.Info,
                    DKimEvaluatorRulesResources.NullPublicKeyConfigured,
                    DKimEvaluatorRulesMarkdownResources.NullPublicKeyConfigured));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;

        public bool IsStopRule => false;
    }
}