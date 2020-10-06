using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Entity.Notifications;

namespace MailCheck.Dkim.Entity.Entity.Notifiers
{
    public class RecordChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDkimEntityConfig _dkimEntityConfig;
        private readonly IEqualityComparer<SelectorRecord> _selectorRecordEqualityComparer;

        public RecordChangedNotifier(IMessageDispatcher dispatcher, IDkimEntityConfig dkimEntityConfig, IEqualityComparer<SelectorRecord> selectorRecordEqualityComparer)
        {
            _dispatcher = dispatcher;
            _dkimEntityConfig = dkimEntityConfig;
            _selectorRecordEqualityComparer = selectorRecordEqualityComparer;
        }

        public void Handle(DkimEntityState state, Common.Messaging.Abstractions.Message message)
        {
            if (message is DkimRecordsPolled polled)
            {
                List<SelectorRecord> currentSelectorRecords = state.Selectors
                    .SelectMany(x => x.Records, (y, z) => new SelectorRecord(y.Selector, z.DnsRecord.Record)).ToList();

                List<SelectorRecord> newSelectorRecords = polled.DkimSelectorRecords
                    .Where(x=>x.Records != null)
                    .SelectMany(x => x.Records, (y, z) => new SelectorRecord(y.Selector, z.Record)).ToList();

                List<SelectorRecord> addedRecords = newSelectorRecords.Except(currentSelectorRecords, _selectorRecordEqualityComparer).ToList();
                if (addedRecords.Any())
                {
                    List<SelectorRecords> selectorRecords = addedRecords
                        .GroupBy(x => x.Selector, y => y.Record)
                        .Select(x => new SelectorRecords(x.Key, x.ToList()))
                        .ToList();

                    DkimRecordAdded dkimDnsRecordAdded = new DkimRecordAdded(state.Id, selectorRecords);
                    _dispatcher.Dispatch(dkimDnsRecordAdded, _dkimEntityConfig.SnsTopicArn);
                }

                List<SelectorRecord> removedRecords = currentSelectorRecords.Except(newSelectorRecords, _selectorRecordEqualityComparer).ToList();
                if (removedRecords.Any())
                {
                    List<SelectorRecords> selectorRecords = removedRecords
                        .GroupBy(x => x.Selector, y => y.Record)
                        .Select(x => new SelectorRecords(x.Key, x.ToList()))
                        .ToList();

                    DkimRecordRemoved dkimDnsRecordRemoved = new DkimRecordRemoved(state.Id, selectorRecords);
                    _dispatcher.Dispatch(dkimDnsRecordRemoved, _dkimEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}