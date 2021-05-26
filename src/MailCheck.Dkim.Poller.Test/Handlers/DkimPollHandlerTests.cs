using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Poller.Config;
using MailCheck.Dkim.Poller.Handlers;
using MailCheck.Dkim.Poller.Services;
using MailCheck.Dkim.Poller.Dns;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Dkim.Poller.Test.Handlers
{
    [TestFixture]
    public class DkimPollHandlerTests
    {
        private IDkimDnsClient _dnsClient;
        private IMessageDispatcher _messageDispatcher;
        private IDkimPollerConfig _config;
        private ILogger<DkimPollerHandler> _log;

        private DkimPollerHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDkimDnsClient>();
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _config = A.Fake<IDkimPollerConfig>();
            _log = A.Fake<ILogger<DkimPollerHandler>>();

            A.CallTo(() => _config.SnsTopicArn).Returns("fakeSnsTopic");

            _sut = new DkimPollerHandler(_dnsClient, _messageDispatcher, _config, _log);
        }

        [Test]
        public async Task ItShouldFetchRecordsForTheSpecifiedDomainAndSelectors()
        {
            List<string> selectors = new List<string> { "foo", "bar" };

            await _sut.Handle(new DkimPollPending("ncsc.gov.uk", 1, selectors));

            A.CallTo(() => _dnsClient.FetchDkimRecords("ncsc.gov.uk", selectors)).MustHaveHappened();
        }


        [Test]
        public async Task ItShouldPublishToTheSpecifiedSnsTopic()
        {
            List<string> selectors = new List<string> { "foo", "bar" };

            await _sut.Handle(new DkimPollPending("ncsc.gov.uk", 1, selectors));

            A.CallTo(() => _dnsClient.FetchDkimRecords("ncsc.gov.uk", selectors)).Returns(new List<DnsResult<DkimSelectorRecords>>());
            A.CallTo(() => _messageDispatcher.Dispatch(A<DkimRecordsPolled>._, "fakeSnsTopic")).MustHaveHappened();
        }
    }
}
