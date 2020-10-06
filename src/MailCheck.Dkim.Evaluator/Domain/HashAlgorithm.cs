using System.Collections.Generic;

namespace MailCheck.Dkim.Evaluator.Domain
{
    public class HashAlgorithm : Tag
    {
        public HashAlgorithm(string value, List<HashAlgorithmValue> hashAlgorithms, bool isImplicit = false) 
            : base(value, isImplicit)
        {
            HashAlgorithms = hashAlgorithms;
        }

        public List<HashAlgorithmValue> HashAlgorithms { get; }
    }
}
