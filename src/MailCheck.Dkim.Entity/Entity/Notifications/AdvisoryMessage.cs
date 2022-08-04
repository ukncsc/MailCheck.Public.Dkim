using MailCheck.Dkim.Contracts.SharedDomain;

namespace MailCheck.Dkim.Entity.Entity.Notifications
{
    public class AdvisoryMessage
    {
        public AdvisoryMessage(string name, MessageType messageType, string text,
            MessageDisplay messageDisplay = MessageDisplay.Standard)
        {
            Name = name;
            MessageType = messageType;
            Text = text;
            MessageDisplay = messageDisplay;
        }

        public string Name { get; }
        public MessageType MessageType { get; }
        public string Text { get; }
        public MessageDisplay MessageDisplay { get; }
    }
}