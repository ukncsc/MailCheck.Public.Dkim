using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.External;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Entity;

namespace MailCheck.Dkim.Entity.Mapping
{
    public static class MessageToDomainMappingExtensions
    {
        public static List<DkimSelector> ToDkimSelectors(this DkimSelectorsSeen selectors)
        {
            return selectors.Selectors.Select(_ => new DkimSelector(_)).ToList();
        }

        public static List<DkimSelector> ToDkimSelectors(this DkimRecordEvaluationResult evaluationResult)
        {
            return evaluationResult.SelectorResults.Select(_ => _.ToDkimSelector()).ToList();
        }

        private static DkimSelector ToDkimSelector(this DkimSelectorResult selectorResult)
        {
            return new DkimSelector(
                selectorResult.Selector.Selector,
                selectorResult.RecordsResults.ToDkimRecords(),
                selectorResult.Selector.CName,
                selectorResult.Selector.PollError != null ? selectorResult.Selector.PollError : null);
        }

        private static List<DkimRecord> ToDkimRecords(this List<RecordResult> results)
        {
            return results?.Select(_ => new DkimRecord(
                new DnsRecord(_.Record, null),
                _.Messages.ToDkimEvaluationMessages())).ToList();
        }

        private static List<Message> ToDkimEvaluationMessages(this List<DkimEvaluatorMessage> evaluatorMessages)
        {
            return evaluatorMessages.Select(_ => new Message(_.Id,
                _.Name,
                _.ErrorType.ToMessageType(),
                _.Message,
                _.MarkDown
                )).ToList();
        }

        private static MessageType ToMessageType(this EvaluationErrorType errorType)
        {
            switch (errorType)
            {
                case (EvaluationErrorType.Error):
                    return MessageType.error;
                case (EvaluationErrorType.Warning):
                    return MessageType.warning;
                default:
                    return MessageType.info;
            }
        }

        private static MessageType ToMessageType(this Contracts.SharedDomain.MessageType errorType)
        {
            switch (errorType)
            {
                case (Contracts.SharedDomain.MessageType.error):
                    return MessageType.error;
                case (Contracts.SharedDomain.MessageType.warning):
                    return MessageType.warning;
                default:
                    return MessageType.info;
            }
        }
    }
}