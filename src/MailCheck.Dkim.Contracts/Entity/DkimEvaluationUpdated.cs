using System;
using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity.Domain;

namespace MailCheck.Dkim.Contracts.Entity
{
    public class DkimEvaluationUpdated : VersionedMessage
    {
        public DkimEvaluationUpdated(string id, int version, List<DkimEvaluationResults> dkimEvaluationResults, DateTime evaluationTime) 
            : base(id, version)
        {
            DkimEvaluationResults = dkimEvaluationResults;
            EvaluationTime = evaluationTime;
        }

        public List<DkimEvaluationResults> DkimEvaluationResults { get; }

        public DateTime EvaluationTime { get; }

        public DkimState State => DkimState.Evaluated;
    }
}
