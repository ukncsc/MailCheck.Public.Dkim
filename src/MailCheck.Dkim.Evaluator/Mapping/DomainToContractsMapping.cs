using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Rules;
using DkimSelectorContract = MailCheck.Dkim.Contracts.SharedDomain.DkimSelector;
using DkimContractRecord = MailCheck.Dkim.Contracts.SharedDomain.DkimRecord;
using DkimRecord = MailCheck.Dkim.Evaluator.Domain.DkimRecord;
using DkimSelector = MailCheck.Dkim.Evaluator.Domain.DkimSelector;

namespace MailCheck.Dkim.Evaluator.Mapping
{
    public static class DomainToContractsMapping
    {
        public static DkimRecordEvaluationResult MapToEvaluationResult(this List<DkimSelectorRecords> selectorRecords, string domain)
        {
            return new DkimRecordEvaluationResult(domain, selectorRecords.Select(MapToSelectorResult).ToList());
        }

        private static DkimSelectorResult MapToSelectorResult(DkimSelectorRecords dkimSelectorRecords)
        {
            return new DkimSelectorResult(ToContractSelector(dkimSelectorRecords.Selector), dkimSelectorRecords.Records.Select(MapToRecordResult).ToList());
        }

        private static RecordResult MapToRecordResult(DkimRecord dkimRecord)
        {
            return new RecordResult(dkimRecord.DnsRecord.Record, dkimRecord.Errors.Select(MapToEvaluatorMessage).ToList());   
        }

        private static DkimEvaluatorMessage MapToEvaluatorMessage(EvaluationError message)
        {
            return new DkimEvaluatorMessage(message.Id, message.ErrorType, message.Message, message.MarkDown);
        }

        private static DkimSelectorContract ToContractSelector(DkimSelector dkimSelector)
        {
            return new DkimSelectorContract(dkimSelector.Selector, dkimSelector.Records.Select(MapToDkimContractRecord).ToList(), dkimSelector.CName, dkimSelector.PollError);
        }

        private static DkimContractRecord MapToDkimContractRecord(DkimRecord dkimRecord)
        {
            return new DkimContractRecord(dkimRecord.DnsRecord, dkimRecord.Errors.Select(MapToMessage).ToList());
        }

        private static Message MapToMessage(EvaluationError message)
        {
            return new Message(message.Id, ToMessageType(message.ErrorType), message.Message, message.MarkDown);
        }

        private static MessageType ToMessageType(EvaluationErrorType type)
        {
            switch (type)
            {
                case EvaluationErrorType.Error:
                    return MessageType.error;
                case EvaluationErrorType.Warning:
                    return MessageType.warning;
                case EvaluationErrorType.Info:
                    return MessageType.info;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}