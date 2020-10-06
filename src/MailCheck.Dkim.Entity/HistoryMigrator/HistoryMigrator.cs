using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Util;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.History;

namespace MailCheck.Dkim.Entity.HistoryMigrator
{
    public interface IHistoryMigrator
    {
        Task Process();
    }

    public class HistoryMigrator : IHistoryMigrator
    {
        private readonly IHistoryDao _historyDao;

        public HistoryMigrator(IHistoryDao historyDao)
        {
            _historyDao = historyDao;
        }

        public async Task Process()
        {
            await Migrate("Has History", _historyDao.GetDomains, _historyDao.GetHistoryForDomains);
            await Migrate("Missing History", _historyDao.GetDomainsWithoutHistory, _historyDao.GetEntityForDomains);
        }

        private async Task Migrate(string name, Func<Task<List<string>>> domainGetter, Func<List<string>, Task<List<DkimEntityState>>> historyGetter)
        {
            int count = 0;

            Console.WriteLine($"Running {name}");

            List<string> domains = await domainGetter();

            Console.WriteLine($"Found {domains.Count} for {name}");

            foreach (IEnumerable<string> batch in domains.Batch(50))
            {
                List<DkimEntityState> states = await historyGetter(batch.ToList());
                List<DkimHistoryEntityState> historyEntityStates = new List<DkimHistoryEntityState>();

                foreach (IGrouping<string, DkimEntityState> domainStates in states.GroupBy(_ => _.Id))
                {
                    List<DkimHistoryRecord> histories = domainStates.OrderBy(_ => _.Version)
                        .Select(_ => new DkimHistoryRecord(GetRecordStartDate(_), null,
                            _.Selectors.Where(s => s.Records != null && s.Records.Any()).Select(s =>
                                new DkimHistoryRecordEntry(s.Selector,
                                    s.Records.Select(r => r.DnsRecord.Record).ToList())).ToList()))
                        .ToList();

                    List<DkimHistoryRecord> distinctHistories = new List<DkimHistoryRecord>();

                    for (int i = 0; i < histories.Count; i++)
                    {
                        DkimHistoryRecord dkimHistoryRecord = histories[i];
                        if (i == 0)
                        {
                            distinctHistories.Add(dkimHistoryRecord);
                        }
                        else
                        {
                            DkimHistoryRecord lastDistinct = distinctHistories.Last();
                            if (!dkimHistoryRecord.Entries.CollectionEqual(lastDistinct.Entries))
                            {
                                //set end date here.
                                lastDistinct.EndDate = dkimHistoryRecord.StartDate;
                                distinctHistories.Add(dkimHistoryRecord);
                            }
                        }
                    }
                    historyEntityStates.Add(new DkimHistoryEntityState(domainStates.Key, distinctHistories));
                }
                await _historyDao.AddHistoryForDomain(historyEntityStates);
                count += batch.Count();
                Console.WriteLine($"Saved history for {count} of {domains.Count} domains for {name}");
            }
        }

        private DateTime GetRecordStartDate(DkimEntityState dkimEntityState)
        {
            List<DateTime> dateTimes = dkimEntityState.LastUpdateSources.Values.ToList();

            dateTimes.Add(dkimEntityState.Created);

            return dateTimes.Max();
        }
    }
}
