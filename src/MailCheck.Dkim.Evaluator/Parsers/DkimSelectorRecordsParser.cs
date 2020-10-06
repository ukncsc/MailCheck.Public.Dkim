using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Parsers
{
    public interface IDkimSelectorRecordsParser
    {
        List<DkimSelectorRecords> Parse(List<DkimSelector> selectors);
    }

    public class DkimSelectorRecordsParser : IDkimSelectorRecordsParser
    {
        private readonly IDkimRecordParser _dkimRecordParser;

        public DkimSelectorRecordsParser(IDkimRecordParser dkimRecordParser)
        {
            _dkimRecordParser = dkimRecordParser;
        }

        public List<DkimSelectorRecords> Parse(List<DkimSelector> selectors)
        {
            //ignore errored records
            return selectors.Where(_ => _.PollError == null).Select(Parse).ToList();
        }

        private DkimSelectorRecords Parse(DkimSelector selector)
        {
            return new DkimSelectorRecords(new DkimSelector(selector.Selector, selector.CName, selector.Records, selector.PollError),
                selector.Records.Select(_ => _dkimRecordParser.Parse(_.DnsRecord)).ToList());
        }
    }
}