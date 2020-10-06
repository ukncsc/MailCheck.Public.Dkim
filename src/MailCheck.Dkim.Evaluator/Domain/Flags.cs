using System.Collections.Generic;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class Flags : Tag
    {
        public Flags(string value, List<FlagTypeValue> flagTypes, bool isImplicit = false) 
            : base(value, isImplicit)
        {
            FlagTypes = flagTypes;
        }

        public List<FlagTypeValue> FlagTypes { get; }
    }
}
