using System;
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
using MoreLinq;

namespace MailCheck.Dkim.Scheduler.Processor
{
    public class DkimPollSchedulerProcessor : IProcess
    {
        private static readonly TimeSpan PublishErrorDelay = TimeSpan.FromSeconds(5);

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

            if (entitiesToUpdate.Count == 0)
            {
                _log.LogInformation($"Found no records to update.");
                return ProcessResult.Stop;
            }

            _log.LogInformation($"Found {entitiesToUpdate.Count} records to update.");

            int totalBatches = 0;
            int successfulBatches = 0;

            foreach (var batchItems in entitiesToUpdate.Batch(10))
            {
                totalBatches++;
                var batch = batchItems.ToArray();

                try
                {
                    var runningTasks = batch.Select(state => _publisher.Publish(state.ToDkimPollMessage(), _config.PublisherConnectionString));
                    await Task.WhenAll(runningTasks);
                }
                catch (Exception e)
                {
                    // If we have a failure publishing we log it, pause for a bit and then continue to next batch
                    _log.LogError(e, $"Exception occurred publishing batch - skipping to next batch");
                    await Task.Delay(PublishErrorDelay);
                    continue;
                }

                try
                {
                    await _dkimPeriodicSchedulerDao.UpdateLastChecked(batch);
                }
                catch (Exception e)
                {
                    // If we have a failure saving to the database we log it and stop further processing
                    _log.LogError(e, "Exception occurred updating batch lastChecked time - halting processing");
                    return ProcessResult.Stop;
                }

                _log.LogInformation($"Processed batch of {batch.Length} items took: {stopwatch.Elapsed}");
                successfulBatches++;
            }

            _log.LogInformation($"Successfully processing {successfulBatches} of {totalBatches} batches took: {stopwatch.Elapsed}");

            return ProcessResult.Continue;
        }
    }
}
