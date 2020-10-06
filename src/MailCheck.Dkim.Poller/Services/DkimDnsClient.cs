using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using MailCheck.Dkim.Contracts.Poller;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.Poller.Services
{
    public interface IDkimDnsClient
    {
        Task<List<DkimSelectorRecords>> FetchDkimRecords(string domain, List<string> selectors);
    }

    public class DkimDnsClient : IDkimDnsClient
    {
        private readonly ILookupClient _lookupClient;
        private readonly ILogger<DkimDnsClient> _log;

        public DkimDnsClient(ILookupClient lookupClient, ILogger<DkimDnsClient> log)
        {
            _lookupClient = lookupClient;
            _log = log;
        }

        public async Task<List<DkimSelectorRecords>> FetchDkimRecords(string domain, List<string> selectors)
        {
            List<DkimSelectorRecords> txtRecords = new List<DkimSelectorRecords>();

            foreach (string selector in selectors)
            {
                string host = $"{selector.Trim().ToLower()}._domainkey.{domain.Trim().ToLower()}";

                DkimSelectorRecords record;

                try
                {
                    IDnsQueryResponse response = await _lookupClient.QueryAsync(host, QueryType.TXT);

                    if (response.HasError)
                    {
                        _log.LogInformation($"Host: {host}");

                        Guid Id1 = Guid.Parse("3956C316-5A86-47D3-B5DD-5F5418C0D5C8");

                        record = new DkimSelectorRecords(Id1, selector, null, null, response.MessageSize,
                            string.Format(DkimServiceResources.DnsLookupFailedErrorMessage, host, response.ErrorMessage),
                            string.Format(DkimServiceMarkdownResources.DnsLookupFailedErrorMessage, host, response.ErrorMessage));
                    }
                    else
                    {
                        Guid Id2 = Guid.Parse("FB710D51-5F2B-47EA-A836-FF4CCFDC30A2");

                        IEnumerable<CNameRecord> cnameRecords = response.Answers.OfType<CNameRecord>().ToList();

                        string dkimCNameRecords = cnameRecords.Any()
                            ? cnameRecords.First().CanonicalName
                            : null;

                        List<DkimTxtRecord> dkimTxtRecords = response.Answers.OfType<TxtRecord>().Select(_ => new DkimTxtRecord(_.Text.Select(r => r.Escape()).ToList())).ToList();

                        record = new DkimSelectorRecords(Id2, selector, dkimTxtRecords, dkimCNameRecords, response.MessageSize);
                    }
                }
                catch (Exception ex)
                {
                    Guid Id3 = Guid.Parse("8DE3D4F8-F1BD-45FF-B289-9A7D23FDC3F4");

                    record = new DkimSelectorRecords(Id3, selector, null, null, 0, ex.Message, DkimServiceMarkdownResources.UnknownErrorWhenPollingErrorMessage);

                    string formatString = $"{{ExceptionType}} occured when polling DKIM record for host {{Host}} {Environment.NewLine} {{StackTrace}}";

                    _log.LogWarning(formatString, host, ex.GetType().Name, ex.StackTrace);
                }

                txtRecords.Add(record);
            }

            return txtRecords;
        }
    }
}
