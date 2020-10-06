using System;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class Tag
    {
        public Tag(string value, bool isImplicit = false)
        {
            IsImplicit = isImplicit;
            Value = value;
        }

        public string Value { get; }

        public bool IsImplicit { get; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}{Environment.NewLine}";
        }
    }
}
