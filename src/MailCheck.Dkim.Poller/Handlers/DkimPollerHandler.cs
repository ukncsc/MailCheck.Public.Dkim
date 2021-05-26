using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Poller.Config;
using MailCheck.Dkim.Poller.Dns;
using MailCheck.Dkim.Poller.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MailCheck.Dkim.Poller.Handlers
{
    public class DkimPollerHandler : IHandle<DkimPollPending>
    {
        private readonly IDkimDnsClient _dnsClient;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ILogger<DkimPollerHandler> _log;
        private readonly IDkimPollerConfig _config;

        public DkimPollerHandler(IDkimDnsClient dnsClient, IMessageDispatcher dispatcher,
            IDkimPollerConfig config, ILogger<DkimPollerHandler> log)
        {
            _dnsClient = dnsClient;
            _dispatcher = dispatcher;
            _log = log;
            _config = config;
        }

        public async Task Handle(DkimPollPending dkimPollPending)
        {
            try
            {
                List<DnsResult<DkimSelectorRecords>> dkimSelectorRecord =
                    await _dnsClient.FetchDkimRecords(dkimPollPending.Id, dkimPollPending.Selectors);

                List<DkimSelectorRecords> dkimSelectorRecords = new List<DkimSelectorRecords>(dkimSelectorRecord.Capacity);

                foreach (DnsResult<DkimSelectorRecords> dnsResult in dkimSelectorRecord)
                {
                    if (dnsResult.IsErrored)
                    {
                        string message =
                            $"Failed DKIM record query for {dkimPollPending.Id} - Selector: {dnsResult.Value.Selector} with error {dnsResult.Error}";

                        _log.LogInformation($"{message} {Environment.NewLine} Audit Trail: {dnsResult.AuditTrail}");
                    }

                    if (dnsResult.Value.Records.Count == 0 ||
                        dnsResult.Value.Records.TrueForAll(y =>
                            string.IsNullOrWhiteSpace(y.Record)))
                    {
                        _log.LogInformation(
                            $"DKIM records are missing or empty for {dkimPollPending.Id}, selectors: {JsonConvert.SerializeObject(dkimPollPending)}, Nameserver: {dnsResult.NameServer}, Audit Trail: {dnsResult.AuditTrail}");
                    }

                    dkimSelectorRecords.Add(dnsResult.Value);
                }


                DkimRecordsPolled dkimRecordsPolled = new DkimRecordsPolled(dkimPollPending.Id, dkimSelectorRecords);

                _log.LogInformation("Polled DKIM selectors for {Domain}", dkimPollPending.Id);

                _dispatcher.Dispatch(dkimRecordsPolled, _config.SnsTopicArn);

                _log.LogInformation("Published DKIM records for {Domain}", dkimPollPending.Id);

            }
            catch (Exception e)
            {
                string error =
                    $"Error occurred polling domain {dkimPollPending.Id}, {Environment.NewLine}";
                _log.LogError(e, error);
                throw;
            }
        }
    }
}
