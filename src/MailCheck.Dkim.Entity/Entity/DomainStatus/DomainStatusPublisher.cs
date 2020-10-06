using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.DomainStatus.Contracts;
using Microsoft.Extensions.Logging;
using SharedDomainMessage = MailCheck.Dkim.Contracts.SharedDomain.Message;

namespace MailCheck.Dkim.Entity.Entity.DomainStatus
{
    public interface IDomainStatusPublisher
    {
        void Publish(DkimRecordEvaluationResult message);
    }

    public class DomainStatusPublisher : IDomainStatusPublisher
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDomainStatusEvaluator _domainStatusEvaluator;
        private readonly IDkimEntityConfig _spfEntityConfig;
        private readonly ILogger<DomainStatusPublisher> _log;

        public DomainStatusPublisher(IMessageDispatcher dispatcher, IDkimEntityConfig spfEntityConfig, IDomainStatusEvaluator domainStatusEvaluator, ILogger<DomainStatusPublisher> log)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
            _log = log;
            _domainStatusEvaluator = domainStatusEvaluator;
        }

        public void Publish(DkimRecordEvaluationResult message)
        {
            if (message.SelectorResults is null || message.SelectorResults.Count == 0)
            {
                return;
            }

            List<Contracts.SharedDomain.DkimSelector> selectors = message.SelectorResults?.Select(x => x.Selector).ToList();

            IEnumerable<SharedDomainMessage> pollErrors = selectors
                .Where(x => x.PollError != null)
                .Select(x => x.PollError);

            IEnumerable<SharedDomainMessage> selectorMessages = selectors
                .Where(x => x.Records != null)
                .SelectMany(x => x.Records)
                .Where(x => x.Messages != null)
                .SelectMany(x => x.Messages);

            IEnumerable<SharedDomainMessage> messages = pollErrors.Concat(selectorMessages);

            List<DkimEvaluatorMessage> recordResults = message.SelectorResults.SelectMany(x => x.RecordsResults)
                .SelectMany(x => x.Messages).ToList();

            Status status = _domainStatusEvaluator.GetStatus(recordResults.ToList(), messages.ToList());

            DomainStatusEvaluation domainStatusEvaluation = new DomainStatusEvaluation(message.Id, "DKIM", status);

            _log.LogInformation($"Publishing DKIM domain status for {message.Id}.");

            _dispatcher.Dispatch(domainStatusEvaluation, _spfEntityConfig.SnsTopicArn);
        }
    }
}
