using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.Common.Util;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.External;
using MailCheck.Dkim.Contracts.Scheduler;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Dao;
using MailCheck.Dkim.Entity.Entity.DomainStatus;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using MailCheck.Dkim.Entity.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Entity.Entity
{
    public class DkimEntity :
        IHandle<DomainCreated>,
        IHandle<DkimSelectorsSeen>,
        IHandle<DkimRecordEvaluationResult>,
        IHandle<DkimRecordsExpired>,
        IHandle<DomainDeleted>
    {
        private readonly IDkimEntityDao _dao;
        private readonly IDkimEntityConfig _config;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IChangeNotifiersComposite _changeNotifierComposite;
        private readonly IDomainStatusPublisher _domainStatusPublisher;
        private readonly IClock _clock;
        private readonly ILogger<DkimEntity> _log;

        private const int DaysBeforeBeingConsideredStale = 2;

        public DkimEntity(IDkimEntityDao dao,
            IDkimEntityConfig config,
            IMessageDispatcher dispatcher,
            IChangeNotifiersComposite changeNotifierComposite,
            IDomainStatusPublisher domainStatusPublisher,
            IClock clock,
            ILogger<DkimEntity> log)
        {
            _dao = dao;
            _config = config;
            _dispatcher = dispatcher;
            _changeNotifierComposite = changeNotifierComposite;
            _domainStatusPublisher = domainStatusPublisher;
            _clock = clock;
            _log = log;
        }

        public async Task Handle(DomainCreated message)
        {
            string id = message.Id.ToLower();

            DkimEntityState state = await _dao.Get(id);

            if (state != null)
            {
                if (state.RecordsLastUpdated == null || state.RecordsLastUpdated.Value.AddDays(DaysBeforeBeingConsideredStale) <= _clock.GetDateTimeUtc())
                {
                    _dispatcher.Dispatch(new DkimEntityCreated(id, state.Version), _config.SnsTopicArn);
                    _log.LogInformation("Published DkimEntityCreated for stale DkimEntity {Id}.", id);
                }
                else
                {
                    throw new MailCheckException($"Cannot handle event {nameof(DomainCreated)} as an up to date DkimEntity already exists for {id}.");
                }
            }
            else
            {
                state = new DkimEntityState(id, 1, DkimState.Created, DateTime.UtcNow, null, null, null);
                await _dao.Save(state);

                _dispatcher.Dispatch(new DkimEntityCreated(id, state.Version), _config.SnsTopicArn);
                _log.LogInformation("Created DkimEntity for {Id}.", id);
            }
        }

        public async Task Handle(DomainDeleted message)
        {
            await _dao.Delete(message.Id);
            _log.LogInformation($"Deleted SPF entity with id: {message.Id}.");
        }


        public async Task Handle(DkimSelectorsSeen message)
        {
            string id = message.Id.ToLower();

            DkimEntityState state = await LoadDkimState(id, message.Timestamp, nameof(DkimSelectorsSeen));

            if (!state.CanUpdate(nameof(DkimSelectorsSeen).ToLower(), message.Timestamp))
            {
                _log.LogInformation($"Cannot handle event DkimSelectorsSeen as newer state exists for {id}.");
                return;
            }

            List<DkimSelector> selectors = message.ToDkimSelectors();
            if (state.UpdateSelectors(selectors, out DkimSelectorsUpdated dkimSelectorsUpdated))
            {
                state.Version++;
                state.UpdateSource(nameof(DkimSelectorsSeen).ToLower(), message.Timestamp);
                await _dao.Save(state);
                _dispatcher.Dispatch(dkimSelectorsUpdated, _config.SnsTopicArn);
                _log.LogInformation("Updated DkimEntity selectors for {Id} to {Selectors}.", id, message.Selectors);
            }
        }

        public async Task Handle(DkimRecordsExpired message)
        {
            string id = message.Id.ToLower();

            DkimEntityState state = await LoadDkimState(id, message.Timestamp, nameof(DkimRecordsExpired));

            if (!state.CanUpdate(nameof(DkimRecordsExpired).ToLower(), message.Timestamp))
            {
                _log.LogInformation($"Cannot handle event DkimRecordsExpired as newer state exists for {id}.");
                return;
            }

            DkimPollPending dkimPollPending = state.UpdatePendingState();

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(dkimPollPending, _config.SnsTopicArn);

            _log.LogInformation("Updated DkimEntity poll pending state for {Id}", id);
        }

        public async Task Handle(DkimRecordEvaluationResult message)
        {
            string id = message.Id.ToLower();

            DkimEntityState state = await LoadDkimState(id, message.Timestamp, nameof(DkimRecordEvaluationResult));

            if (!state.CanUpdate(nameof(DkimRecordEvaluationResult).ToLower(), message.Timestamp))
            {
                _log.LogInformation("Cannot handle event DkimRecordEvaluationResult as newer state exists for {id}.");
                return;
            }

            _changeNotifierComposite.Handle(state, message);
            _domainStatusPublisher.Publish(message);

            List<DkimSelector> selectors = message.ToDkimSelectors();

            state.UpdateRecords(selectors, message.Timestamp);

            DkimEvaluationUpdated evaluationUpdated = state.UpdateEvaluations(message.Timestamp);

            state.Version++;
            state.UpdateSource(nameof(DkimRecordEvaluationResult).ToLower(), message.Timestamp);
            await _dao.Save(state);
            _dispatcher.Dispatch(evaluationUpdated, _config.SnsTopicArn);
            _log.LogInformation("Updated DkimEntity evaluation results for {Id}", id);
        }

        private async Task<DkimEntityState> LoadDkimState(string id, DateTime timestamp, string messageType)
        {
            DkimEntityState state = await _dao.Get(id);
            if (state == null)
            {
                _dispatcher.Dispatch(new DomainMissing(id), _config.SnsTopicArn);
                _log.LogError("DomainMissing dispatched as DkimEntity does not exist for {Id}.", id);
                _log.LogError("Ignoring {EventName} as DkimEntity does not exist for {Id}.", messageType, id);
                throw new MailCheckException($"Cannot handle event {messageType} as DkimEntity doesnt exists for {id}.");
            }

            return state;
        }
    }
}