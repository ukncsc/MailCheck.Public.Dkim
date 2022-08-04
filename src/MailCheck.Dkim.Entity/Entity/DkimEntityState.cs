using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Entity.Mapping;

namespace MailCheck.Dkim.Entity.Entity
{
    public class DkimEntityState : StateBase
    {
        public DkimEntityState(string id, 
            int version, 
            DkimState state,
            DateTime created, 
            DateTime? recordsLastUpdated, 
            DateTime? evaluationsLastUpdated, 
            List<DkimSelector> selectors)
        {
            Id = id;
            Version = version;
            State = state;
            Created = created;
            RecordsLastUpdated = recordsLastUpdated;
            EvaluationsLastUpdated = evaluationsLastUpdated;
            Selectors = selectors ?? new List<DkimSelector>();
        }

        public virtual string Id { get; }
        public virtual int Version { get; set; }
        public virtual DkimState State { get; private set; }
        public virtual DateTime Created { get; }
        public virtual DateTime? RecordsLastUpdated { get; private set; }
        public virtual DateTime? EvaluationsLastUpdated { get; private set; }
        public virtual List<DkimSelector> Selectors { get; }

        public virtual bool UpdateSelectors(List<DkimSelector> selectors, out DkimSelectorsUpdated dkimSelectorsUpdated)
        {
            dkimSelectorsUpdated = null;

            List<DkimSelector> newSelectors = selectors.Where(_ => !Selectors.Select(s => s.Selector).Contains(_.Selector)).ToList();

            if (newSelectors.Any())
            {
                Selectors.AddRange(newSelectors);

                //publish all state
                dkimSelectorsUpdated = Selectors.ToDkimSelectorsUpdated(Id, Version);
            }

            return newSelectors.Any();
        }

        public virtual void UpdateRecords(List<DkimSelector> selectors, DateTime updatedTime)
        {
            Selectors.Clear();

            Selectors.AddRange(selectors.Where(_ => _.PollError == null).ToList());

            foreach (DkimSelector selector in Selectors)
            {
                if (!selector.Records.Any())
                {
                    Guid Error1Id = Guid.Parse("9EB03982-274E-4015-BC04-2436C54F9E64");

                    selector.UpdatePollError(
                        new Message(Error1Id, "mailcheck.dkim.noDnsTxtRecords", MessageType.error, string.Format(DkimEntityResources.NoDnsTxtRecordsErrorMessage, selector.Selector, Id),
                        string.Format(DkimEntityMarkdownResources.NoDnsTxtRecordsErrorMessage, selector.Selector, Id)));
                }
                else
                {
                    selector.UpdateCName(selector.CName);
                    selector.UpdateRecords(selector.Records);
                }
            }

            RecordsLastUpdated = updatedTime;
        }

        public virtual DkimPollPending UpdatePendingState()
        {
            State = DkimState.PollPending;

            return new DkimPollPending(Id, Version, Selectors.Select(_ => _.Selector).ToList());
        }

        public virtual DkimEvaluationUpdated UpdateEvaluations(DateTime updatedTime)
        {
            foreach (DkimSelector selector in Selectors)
            {
                selector.UpdateEvaluations(selector.Records);
            }

            EvaluationsLastUpdated = updatedTime;
            State = DkimState.Evaluated;

            //publish all state
            return Selectors.ToDkimEvaluationUpdated(Id, Version, updatedTime);
        }
    }
}