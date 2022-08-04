using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class SelectorMessage
    {
        public string Selector { get; }
        public Message Message { get; }

        public SelectorMessage(string selector, Message message)
        {
            Selector = selector;
            Message = message;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Selector != null ? Selector.GetHashCode() : 0) * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }
    }
}