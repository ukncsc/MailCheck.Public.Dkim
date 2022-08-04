using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Implicit;
using MailCheck.Dkim.Evaluator.Rules;
using DkimRecord = MailCheck.Dkim.Evaluator.Domain.DkimRecord;

namespace MailCheck.Dkim.Evaluator.Parsers
{
    public interface IDkimRecordParser
    {
        DkimRecord Parse(string selector, DnsRecord record, string cname);
    }

    public class DkimRecordParser : IDkimRecordParser
    {
        private const char Separator = ';';
        private readonly ITagParser _tagParser;
        private readonly IImplicitProvider<Tag> _implicitProvider;
        private readonly IEvaluator<DkimEvaluationObject> _ruleEvaluator;

        public DkimRecordParser(
            ITagParser tagParser,
            IImplicitProvider<Tag> implicitProvider,
            IEvaluator<DkimEvaluationObject> ruleEvaluator)
        {
            _tagParser = tagParser;
            _implicitProvider = implicitProvider;
            _ruleEvaluator = ruleEvaluator;
        }

        public DkimRecord Parse(string selector, DnsRecord record, string cname)
        {
            List<string> stringTags = record.Record.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Trim())
                .Where(_ => _ != string.Empty)
                .ToList();

            EvaluationResult<List<Tag>> evaluatedTags = _tagParser.Parse(selector, stringTags);

            List<Tag> implicitTags = _implicitProvider.GetImplicitValues(evaluatedTags.Item);

            DkimRecord dkimRecord = new DkimRecord(record, evaluatedTags.Item.Concat(implicitTags).ToList(),
                evaluatedTags.Errors);

            if (evaluatedTags.Errors != null && evaluatedTags.Errors.All(x => x.ErrorType != EvaluationErrorType.Error))
            {
                EvaluationResult<DkimEvaluationObject> evaluationResult = _ruleEvaluator.Evaluate(new DkimEvaluationObject(selector, dkimRecord, cname)).Result;

                dkimRecord.Errors.AddRange(evaluationResult.Errors);
            }

            return dkimRecord;
        }
    }
}
