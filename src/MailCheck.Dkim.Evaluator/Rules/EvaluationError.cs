using MailCheck.Dkim.Contracts.Evaluator.Domain;
using System;

namespace MailCheck.Dkim.Evaluator.Rules
{
    public class EvaluationError
    {
        public EvaluationError(Guid id, EvaluationErrorType errorType, string message, string markDown)
        {
            Id = id;
            ErrorType = errorType;
            Message = message;
            MarkDown = markDown;
        }

        public Guid Id { get; }
        public EvaluationErrorType ErrorType { get; }

        public string Message { get; }
        public string MarkDown { get; }

        protected bool Equals(EvaluationError other)
        {
            return ErrorType == other.ErrorType && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EvaluationError)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)ErrorType * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{nameof(ErrorType)}: {ErrorType}, {nameof(Message)}: {Message}";
        }
    }
}