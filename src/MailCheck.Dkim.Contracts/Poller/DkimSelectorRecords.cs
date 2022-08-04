using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Contracts.Poller
{
    public class DkimSelectorRecords
    {
        public DkimSelectorRecords(Guid id, string selector, List<DkimTxtRecord> records, string cName, int messageSize, string errorName = null, string error = null, string errorMarkDown = null, DateTime? checkd = null)
        {
            Id = id;
            Selector = selector;
            Records = records ?? new List<DkimTxtRecord>();
            CName = cName;
            MessageSize = messageSize;
            ErrorName = errorName;
            Error = error;
            ErrorMarkDown = errorMarkDown;
            Checked = checkd ?? DateTime.UtcNow;
        }

        public Guid Id { get; }

        public string Selector { get; }
            
        public List<DkimTxtRecord> Records { get; }
        public string CName { get; }

        public int MessageSize { get; }
        public string ErrorName { get; }
        public string Error { get; }
        public string ErrorMarkDown { get; }
        public DateTime Checked { get; }
    }
}
