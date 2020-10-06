using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MailCheck.Dkim.Entity.Entity
{
    public class Message
    {
        public Message(Guid id, string text, string markDown, MessageType messageType)
        {
            Id = id;
            Text = text;
            MarkDown = markDown;
            MessageType = messageType;
        }

        public Guid Id { get; }
        public string Text { get; }
        public string MarkDown { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType MessageType { get; }

        protected bool Equals(Message other)
        {
            return Id == other.Id && MessageType == other.MessageType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Message) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (int) MessageType;
            }
        }
    }
}