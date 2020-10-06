namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class SelectorRecord
    {
        public string Selector { get; }
        public string Record { get; }

        public SelectorRecord(string selector, string record)
        {
            Selector = selector;
            Record = record;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Selector != null ? Selector.GetHashCode() : 0) * 397) ^ (Record != null ? Record.GetHashCode() : 0);
            }
        }
    }
}