using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MailCheck.Dkim.Contracts.Entity.Domain
{
    public class DkimEvaluationMessage
    {
        public DkimEvaluationMessage(Guid id, string message, string markDown, DkimEvaluationMessageType messageType)
        {
            Id = id;
            Message = message;
            MarkDown = markDown;
            MessageType = messageType;
        }

        public Guid Id { get; }
        public string Message { get; }
        public string MarkDown { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DkimEvaluationMessageType MessageType { get; }
    }
}
