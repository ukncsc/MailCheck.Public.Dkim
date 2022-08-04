using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Mapping;
using ErrorMessage = MailCheck.Dkim.Contracts.SharedDomain.Message;
using Message = MailCheck.Dkim.Contracts.SharedDomain.Message;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IFindingsChangedNotifier _findingsChangedNotifier;
        private readonly IDkimEntityConfig _dkimEntityConfig;

        public FindingsChangedNotifier(
            IMessageDispatcher dispatcher,
            IFindingsChangedNotifier findingsChangedNotifier,
            IDkimEntityConfig dkimEntityConfig)
        {
            _dispatcher = dispatcher;
            _findingsChangedNotifier = findingsChangedNotifier;
            _dkimEntityConfig = dkimEntityConfig;
        }

        public void Handle(DkimEntityState state, Common.Messaging.Abstractions.Message message)
        {
            string messageId = state.Id.ToLower();
           
            if (message is DkimRecordEvaluationResult evaluationResult)
            {
                List<DkimSelector> newRecordsIgnoringEmptyResults = evaluationResult.ToDkimSelectors().Where(x => x.Records != null).ToList();

                FindingsChanged findingsChanged = _findingsChangedNotifier.Process(messageId, "DKIM",
                    ExtractFindings(messageId, state.Selectors),
                    ExtractFindings(messageId, newRecordsIgnoringEmptyResults));
                _dispatcher.Dispatch(findingsChanged, _dkimEntityConfig.SnsTopicArn);
            }
        }

        private IList<Finding> ExtractFindings(string domain, List<DkimSelector> dkimSelectors)
        {
            List<Finding> findings = new List<Finding>();

            foreach (DkimSelector selectorResult in dkimSelectors)
            {
                string selector = selectorResult.Selector;

                List<Message> messages = new List<Message>();

                messages.Add(selectorResult.PollError);
                messages.AddRange(selectorResult.Records?.Where(x => x.Messages != null).SelectMany(x => x.Messages));

                findings.AddRange(ExtractFindingsFromMessages(domain, selector, messages));
            }

            return findings;
        }

        private List<Finding> ExtractFindingsFromMessages(string domain, string selector, List<ErrorMessage> messages)
        {
            List<Finding> findings = messages.Where(msg => msg != null).Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_dkimEntityConfig.WebUrl}/app/domain-security/{domain}/dkim/{selector}",
                Title = msg.Text,
                EntityUri = $"domain:{domain}|selector:{selector}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();

            return findings;
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
            [MessageType.positive] = "Positive",
        };
    }
}