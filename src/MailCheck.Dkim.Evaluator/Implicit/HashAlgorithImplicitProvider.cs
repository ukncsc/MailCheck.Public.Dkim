using System.Collections.Generic;
using MailCheck.Dkim.Evaluator.Domain;

namespace MailCheck.Dkim.Evaluator.Implicit
{
    public class HashAlgorithImplicitProvider : ImplicitTagProviderStrategyBase<HashAlgorithm>
    {
        public HashAlgorithImplicitProvider() : base(t => new HashAlgorithm("sha1:sha256", 
            new List<HashAlgorithmValue>
        {
            new HashAlgorithmValue("sha1", HashAlgorithmType.Sha1),
            new HashAlgorithmValue("sha256", HashAlgorithmType.Sha256)
        }, true)){}
    }
}