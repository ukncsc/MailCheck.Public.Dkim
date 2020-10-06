using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Evaluator;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Evaluator.Config;
using MailCheck.Dkim.Evaluator.Mapping;
using MailCheck.Dkim.Evaluator.Parsers;
using Microsoft.Extensions.Logging;
using DkimSelector = MailCheck.Dkim.Evaluator.Domain.DkimSelector;
using DkimSelectorRecords = MailCheck.Dkim.Evaluator.Domain.DkimSelectorRecords;

namespace MailCheck.Dkim.Evaluator.Handler
{
    public class DkimSelectorHandler : IHandle<DkimRecordsPolled>
    {
        private readonly IDkimSelectorRecordsParser _dkimSelectorRecordsParser;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDkimEvaluatorConfig _config;
        private readonly ILogger<DkimSelectorHandler> _log;

        public DkimSelectorHandler(
            IDkimSelectorRecordsParser dkimSelectorRecordsParser, 
            IMessageDispatcher dispatcher,
            IDkimEvaluatorConfig config, 
            ILogger<DkimSelectorHandler> log)
        {
            _dkimSelectorRecordsParser = dkimSelectorRecordsParser;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public Task Handle(DkimRecordsPolled message)
        {
            List<DkimSelector> selectors = message.ToDkimRecords();

            _log.LogInformation("Evaluating DKIM selectors record for {Domain}", message.Id);

            List<DkimSelectorRecords> dkimSelectorRecords = _dkimSelectorRecordsParser.Parse(selectors);

            _log.LogInformation("Evaluated DKIM selectors record for {Domain}", message.Id);

            DkimRecordEvaluationResult dkimRecordEvaluationResult = dkimSelectorRecords.MapToEvaluationResult(message.Id);

            _dispatcher.Dispatch(dkimRecordEvaluationResult, _config.SnsTopicArn);

            _log.LogInformation("Published DKIM results for {Domain}", message.Id);

            return Task.CompletedTask;
        }
    }
}
