using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Poller.Dns;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Poller.Services
{
    public interface IDkimDnsClient
    {
        Task<List<DnsResult<DkimSelectorRecords>>> FetchDkimRecords(string domain, List<string> selectors);
    }


    public class DkimDnsClient : IDkimDnsClient
    {
        private const string SERV_FAILURE_ERROR = "Server Failure";
        private const string NON_EXISTENT_DOMAIN_ERROR = "Non-Existent Domain";
        private readonly ILookupClient _lookupClient;
        private readonly ILogger<DkimDnsClient> _log;
        private const int maxLookups = 40;

        public DkimDnsClient(ILookupClient lookupClient, ILogger<DkimDnsClient> log)
        {
            _lookupClient = lookupClient;
            _log = log;
        }

        public async Task<List<DnsResult<DkimSelectorRecords>>> FetchDkimRecords(string domain, List<string> selectors)
        {
            List<DnsResult<DkimSelectorRecords>> txtRecords = new List<DnsResult<DkimSelectorRecords>>();

            long totalSelectors = selectors.Count;

            int lookups = (int)Math.Min(totalSelectors, maxLookups);

            for (int i = 0; i < lookups; i++)
            {
                string selector = selectors[i];

                string host = $"{selector.Trim().ToLower()}._domainkey.{domain.Trim().ToLower()}";

                DnsResult<DkimSelectorRecords> record;

                try
                {
                    IDnsQueryResponse response = await _lookupClient.QueryAsync(host, QueryType.TXT);

                    if (response.HasError && response.ErrorMessage != NON_EXISTENT_DOMAIN_ERROR &&
                        response.ErrorMessage != SERV_FAILURE_ERROR)
                    {
                        _log.LogInformation($"Host: {host}");

                        Guid Id1 = Guid.Parse("3956C316-5A86-47D3-B5DD-5F5418C0D5C8");

                        record = new DnsResult<DkimSelectorRecords>(
                            new DkimSelectorRecords(Id1, selector, null, null, response.MessageSize,
                                string.Format(DkimServiceResources.DnsLookupFailedErrorMessage, host,
                                    response.ErrorMessage),
                                string.Format(DkimServiceMarkdownResources.DnsLookupFailedErrorMessage, host,
                                    response.ErrorMessage)), response.NameServer.ToString(), response.AuditTrail);
                    }
                    else
                    {
                        Guid Id2 = Guid.Parse("FB710D51-5F2B-47EA-A836-FF4CCFDC30A2");

                        IEnumerable<CNameRecord> cnameRecords = response.Answers.OfType<CNameRecord>().ToList();

                        string dkimCNameRecords = cnameRecords.Any()
                            ? cnameRecords.First().CanonicalName
                            : null;

                        List<DkimTxtRecord> dkimTxtRecords = response.Answers.OfType<TxtRecord>()
                            .Select(_ => new DkimTxtRecord(_.Text.Select(r => r.Escape()).ToList())).ToList();

                        record = new DnsResult<DkimSelectorRecords>(new DkimSelectorRecords(Id2, selector,
                            dkimTxtRecords, dkimCNameRecords, response.MessageSize));
                    }
                }
                catch (Exception ex)
                {
                    Guid Id3 = Guid.Parse("8DE3D4F8-F1BD-45FF-B289-9A7D23FDC3F4");

                    record = new DnsResult<DkimSelectorRecords>(new DkimSelectorRecords(Id3, selector, null, null, 0,
                        ex.Message, DkimServiceMarkdownResources.UnknownErrorWhenPollingErrorMessage));

                    string warning = $"Exception occurred when polling DKIM record for host {host}";

                    _log.LogWarning(ex, warning);
                }

                txtRecords.Add(record);
            }

            return txtRecords;
        }
    }
}
