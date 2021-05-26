using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Poller.Services;
using MailCheck.Dkim.Poller.Dns;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dkim.Poller.Test.Services
{
    [TestFixture]
    public class DkimDnsClientTests
    {
        private DkimDnsClient _client;
        private ILookupClient _lookupClient;
        private ILogger<DkimDnsClient> _logger;
        private IDnsQueryResponse _dnsResponse;

        [SetUp]
        public void SetUp()
        {
            _lookupClient = A.Fake<ILookupClient>();
            _logger = A.Fake<ILogger<DkimDnsClient>>();
            _dnsResponse = A.Fake<IDnsQueryResponse>();

            _client = new DkimDnsClient(_lookupClient, _logger);
        }

        [Test]
        public async Task ItShouldHaveAnErrorIfThereIsAnExceptionWhenGettingTheRecord()
        {
            A.CallTo(_lookupClient).Throws(new Exception("Invalid domain name"));

            List<DnsResult<DkimSelectorRecords>> response = await _client.FetchDkimRecords("ncsc.gov.uk", new List<string> { "selector1" });

            Assert.That(response[0].Value.Records, Is.Empty);
            Assert.AreEqual(response[0].Value.Error, "Invalid domain name");
        }

        [Test]
        public async Task ItShouldContainTheRecordsWhenSuccessful()
        {
            string[] record1 = {"record1"};
            string[] record2 = {"record2-part1 ","record2-part2"};
            A.CallTo(() => _dnsResponse.Answers)
             .Returns(CreateDnsResponse(record1, record2));

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._))
                .Returns(Task.FromResult(_dnsResponse));

            List<DnsResult<DkimSelectorRecords>> response = await _client.FetchDkimRecords("ncsc.gov.uk", new List<string> { "selector1" });

            Assert.AreEqual(response[0].Value.Records[0].Record, "record1");
            Assert.That(response[0].Value.Records[0].RecordParts.SequenceEqual(record1), Is.True);
            Assert.AreEqual(response[0].Value.Records[1].Record, "record2-part1 record2-part2");
            Assert.That(response[0].Value.Records[1].RecordParts.SequenceEqual(record2), Is.True);
            Assert.That(response[0].Error, Is.Null);
        }

        [Test]
        public async Task ItShouldContainEmptyRecordsWhenThereIsNoDkim()
        {
            A.CallTo(() => _dnsResponse.Answers)
             .Returns(CreateDnsResponse());

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._))
                .Returns(Task.FromResult(_dnsResponse));

            List<DnsResult<DkimSelectorRecords>> response = await _client.FetchDkimRecords("ncsc.gov.uk", new List<string> { "selector1" });

            Assert.AreEqual(response[0].Value.Records.Count, 0);
            Assert.That(response[0].Error, Is.Null);
        }

        [Test]
        public async Task ItShouldContainErrorClientReturnsNonSuccessCode()
        {
            A.CallTo(() => _dnsResponse.HasError).Returns(true);
            A.CallTo(() => _dnsResponse.ErrorMessage).Returns("Non-Existent Domain");

            A.CallTo(() => _lookupClient.QueryAsync(A<string>._, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._))
                .Returns(Task.FromResult(_dnsResponse));

            List<DnsResult<DkimSelectorRecords>> response = await _client.FetchDkimRecords("ncsc.gov.uk", new List<string> { "selector1" });

            Assert.That(response[0].Value.Records, Is.Empty);
        }


        private List<DnsResourceRecord> CreateDnsResponse(params string[][] args) =>

            args.Select((v, i) => new TxtRecord(
                new ResourceRecordInfo("ncsc.gov.uk", ResourceRecordType.TXT, QueryClass.CS, 0, 0),
                args[i].ToArray(), 
                args[i].ToArray()))
                .Cast<DnsResourceRecord>()
                .ToList();
    }
}
