using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.Poller
{
    public class DkimTxtRecord
    {
        public DkimTxtRecord(List<string> recordParts)
        {
            RecordParts = recordParts ?? new List<string>();
            Record = string.Join(string.Empty, recordParts);
        }

        public List<string> RecordParts { get; }

        public string Record { get; }
    }
}