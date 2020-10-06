using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Mapping;

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
            }
        }

        private List<SelectorMessage> CreateFlattenedSelectorMessages(IEnumerable<DkimSelector> dkimSelectors)
        {
            return dkimSelectors
                .SelectMany(x => x.Records ?? new List<DkimRecord>(), (y, z) => new { y.Selector, z.EvaluationMessages })
                .SelectMany(x => x.EvaluationMessages ?? new List<Message>(), (x, y) => new SelectorMessage(x.Selector, y))
                .ToList();
        }

        private List<SelectorMessages> CreateExclusiveSelectorMessages(IEnumerable<SelectorMessage> first, IEnumerable<SelectorMessage> second)
        {
            return first
                .Except(second, _selectorMessageEqualityComparer)
                .GroupBy(x => x.Selector, y => y.Message)
                .Select(x => new SelectorMessages(x.Key, x.Select(y => new AdvisoryMessage((Notifications.MessageType)y.MessageType, y.Text)).ToList()))
                .ToList();
        }
    }
}