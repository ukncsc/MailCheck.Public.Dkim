using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Entity.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Entity.Mapping
{
    public static class DomainToMessageMappingExtensions
    {
        public static DkimSelectorsUpdated ToDkimSelectorsUpdated(this List<DkimSelector> selectors, string id, int version)
        {
            return new DkimSelectorsUpdated(id, version, selectors.Select(_ => _.Selector).ToList());
        }
        public static DkimEvaluationUpdated ToDkimEvaluationUpdated(this List<DkimSelector> selectors, string id,
            int version, DateTime evaluationTime)
        {
            return new DkimEvaluationUpdated(id, version, selectors.Select(_ => _.ToDkimEvaluationResults()).ToList(), evaluationTime);
        }
        
        private static DkimEvaluationResults ToDkimEvaluationResults(this DkimSelector selector)
        {
            return new DkimEvaluationResults(
                selector.Selector,
                selector.Records?.Select(_ => _.ToDkimEvaluationRecord()).ToList());
        }

        private static DkimEvaluationRecord ToDkimEvaluationRecord(this DkimRecord record)
        {
            return new DkimEvaluationRecord(
                record.DnsRecord.Record, 
                record.Messages?.Select(_ => _.ToDkimEvaluationMessage()).ToList());
        }

        private static DkimEvaluationMessage ToDkimEvaluationMessage(this Message message)
        {
            return new DkimEvaluationMessage(message.Id, message.Text, message.MarkDown, (DkimEvaluationMessageType)message.MessageType);
        }
    }
}