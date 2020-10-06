using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;
using MailCheck.Dkim.Scheduler.Utils;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Scheduler.Processor
{
    public class DkimPollSchedulerProcessor : IProcess
    {
        private readonly IDkimPeriodicSchedulerDao _dkimPeriodicSchedulerDao;
        private readonly IMessagePublisher _publisher;
        private readonly IDkimSchedulerConfig _config;
        private readonly ILogger<DkimPollSchedulerProcessor> _log;

        public DkimPollSchedulerProcessor(
            IDkimPeriodicSchedulerDao dkimPeriodicSchedulerDao, 
            IMessagePublisher publisher,
            IDkimSchedulerConfig config, 
            ILogger<DkimPollSchedulerProcessor> log)
        {
            _dkimPeriodicSchedulerDao = dkimPeriodicSchedulerDao;
            _publisher = publisher;
            _config = config;
            _log = log;
        }

        public async Task<ProcessResult> Process()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<DkimSchedulerState> entitiesToUpdate = await _dkimPeriodicSchedulerDao.GetDkimRecordsToUpdate();

            _log.LogInformation($"Found {entitiesToUpdate.Count} records to update.");

            if (entitiesToUpdate.Any())
            {
                entitiesToUpdate.ForEach(async _ => await _publisher.Publish(_.ToDkimPollMessage(), _config.PublisherConnectionString));
                await _dkimPeriodicSchedulerDao.UpdateLastChecked(entitiesToUpdate);
                _log.LogInformation($"Processing {entitiesToUpdate.Count} took: {stopwatch.Elapsed}");
            }

            stopwatch.Stop();

            return entitiesToUpdate.Any() ? ProcessResult.Continue : ProcessResult.Stop;
        }
    }
}
