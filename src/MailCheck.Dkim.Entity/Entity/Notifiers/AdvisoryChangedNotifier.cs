﻿using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Mapping;
using Message = MailCheck.Dkim.Contracts.SharedDomain.Message;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public class AdvisoryChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDkimEntityConfig _dkimEntityConfig;
        private readonly IEqualityComparer<SelectorMessage> _selectorMessageEqualityComparer;

        public AdvisoryChangedNotifier(IMessageDispatcher dispatcher, IDkimEntityConfig dkimEntityConfig, IEqualityComparer<SelectorMessage> selectorMessageEqualityComparer)
        {
            _dispatcher = dispatcher;
            _dkimEntityConfig = dkimEntityConfig;
            _selectorMessageEqualityComparer = selectorMessageEqualityComparer;
        }

        public void Handle(DkimEntityState state, Common.Messaging.Abstractions.Message message)
        {
            if (message is DkimRecordEvaluationResult evaluationResult)
            {
                List<DkimSelector> newRecordsIgnoringEmptyResults = evaluationResult.ToDkimSelectors().Where(x => x.Records != null).ToList();

                List<SelectorMessage> currentSelectorMessages = CreateFlattenedSelectorMessages(state.Selectors);
                List<SelectorMessage> newSelectorMessages = CreateFlattenedSelectorMessages(newRecordsIgnoringEmptyResults);

                List<SelectorMessages> addedMessages = CreateExclusiveSelectorMessages(newSelectorMessages, currentSelectorMessages);
                if (addedMessages.Any())
                {
                    DkimAdvisoryAdded dkimAdvisoryAdded = new DkimAdvisoryAdded(state.Id, addedMessages);
                    _dispatcher.Dispatch(dkimAdvisoryAdded, _dkimEntityConfig.SnsTopicArn);
                }

                List<SelectorMessages> removedMessages = CreateExclusiveSelectorMessages(currentSelectorMessages, newSelectorMessages);
                if (removedMessages.Any())
                {
                    DkimAdvisoryRemoved dkimAdvisoryRemoved = new DkimAdvisoryRemoved(state.Id, removedMessages);
                    _dispatcher.Dispatch(dkimAdvisoryRemoved, _dkimEntityConfig.SnsTopicArn);
                }

                List<SelectorMessages> sustainedMessages = CreateIntersectionSelectorMessages(currentSelectorMessages, newSelectorMessages);

                if (sustainedMessages.Any())
                {
                    DkimAdvisorySustained dkimAdvisorySustained = new DkimAdvisorySustained(state.Id, sustainedMessages);
                    _dispatcher.Dispatch(dkimAdvisorySustained, _dkimEntityConfig.SnsTopicArn);
                }
            }
        }

        private List<SelectorMessages> CreateExclusiveSelectorMessages(IEnumerable<SelectorMessage> first, IEnumerable<SelectorMessage> second)
        {
            return first
                .Except(second, _selectorMessageEqualityComparer)
                .GroupUpSelectorMessages()
                .ToList();
        }

        private List<SelectorMessages> CreateIntersectionSelectorMessages(IEnumerable<SelectorMessage> first, IEnumerable<SelectorMessage> second)
        {
            return first
                .Intersect(second, _selectorMessageEqualityComparer)
                .GroupUpSelectorMessages()
                .ToList();
        }

        private List<SelectorMessage> CreateFlattenedSelectorMessages(IEnumerable<DkimSelector> dkimSelectors)
        {
            return dkimSelectors
                .SelectMany(x => x.Records ?? new List<DkimRecord>(), (y, z) => new { y.Selector, z.Messages })
                .SelectMany(x => x.Messages ?? new List<Message>(), (x, y) => new SelectorMessage(x.Selector, y))
                .ToList();
        }
    }
}