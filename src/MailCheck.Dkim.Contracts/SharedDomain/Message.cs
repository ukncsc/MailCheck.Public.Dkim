using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MailCheck.Dkim.Contracts.SharedDomain
{
    public class Message : IEquatable<Message>
    {
        public Message(Guid id, string name, MessageType messageType, string text, string markDown)
        {
            Id = id;
            Name = name;
            MessageType = messageType;
            Text = text;
            MarkDown = markDown;
        }

        public Guid Id { get; }
        public string Name { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType MessageType { get; }

        public string Text { get; }
        public string MarkDown { get; }

        public bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MessageType == other.MessageType &&
                   string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Message)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MessageType.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Message left, Message right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Message left, Message right)
        {
            return !Equals(left, right);
        }
    }
}
