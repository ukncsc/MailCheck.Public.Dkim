using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MailCheck.Dkim.Contracts.Evaluator.Domain
{
    public class DkimEvaluatorMessage
    {
        public DkimEvaluatorMessage(Guid id, string name, EvaluationErrorType errorType, string message, string markDown)
        {
            Id = id;
            Name = name;
            ErrorType = errorType;
            Message = message;
            MarkDown = markDown;
        }

        public Guid Id { get; }
        public string Name { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EvaluationErrorType ErrorType { get; }

        public string Message { get; }
        public string MarkDown { get; }
    }
}