using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Poller.Config;
using MailCheck.Dkim.Poller.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MailCheck.Dkim.Poller.Handlers
{
    public class DkimPollerHandler : IHandle<DkimPollPending>
    {
        private readonly IDkimDnsClient _dnsClient;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDkimPollerConfig _config;
        private readonly ILogger<DkimPollerHandler> _log;

        public DkimPollerHandler(IDkimDnsClient dnsClient, IMessageDispatcher dispatcher,
            IDkimPollerConfig config, ILogger<DkimPollerHandler> log)
        {
            _dnsClient = dnsClient;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(DkimPollPending message)
        {
            try
            {
                List<DkimSelectorRecords> dkimSelectorRecords =
                    await _dnsClient.FetchDkimRecords(message.Id, message.Selectors);

                if (!_config.AllowNullResults && dkimSelectorRecords.TrueForAll(x =>
                         x.Records.Count == 0 || x.Records.TrueForAll(y => string.IsNullOrWhiteSpace(y.Record))))
                {
                    throw new Exception($"Unable to retrieve DKIM records for {message.Id}, selectors: {JsonConvert.SerializeObject(message)}");
                }

                DkimRecordsPolled dkimRecordsPolled = new DkimRecordsPolled(message.Id, dkimSelectorRecords);

                _log.LogInformation("Polled DKIM selectors for {Domain}", message.Id);

                _dispatcher.Dispatch(dkimRecordsPolled, _config.SnsTopicArn);

                _log.LogInformation("Published DKIM records for {Domain}", message.Id);

            }
            catch (Exception e)
            {
                string error = $"Error occurred polling domain {message.Id}";
                _log.LogError(e, error);
                throw;
            }
        }
    }
}
