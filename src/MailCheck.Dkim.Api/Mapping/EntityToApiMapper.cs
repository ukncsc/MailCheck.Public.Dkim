using System.Collections.Generic;
using System.Linq;
using MailCheck.Dkim.Api.Domain;

namespace MailCheck.Dkim.Api.Mapping
{
    public interface IEntityToApiMapper
    {
        DkimResponse ToDkimResponse(EntityDkimEntityState state);
    }

    public class EntityToApiMapper : IEntityToApiMapper
    {
        public DkimResponse ToDkimResponse(EntityDkimEntityState state)
        {
            return new DkimResponse(
                state.Id,
                (State)state.State,
                state.Selectors
                    .Where(_ => _.PollError == null && _.Records.Any())
                    .Select(ToDkimSelector)
                    .ToList(),
                state.RecordsLastUpdated);
        }

        private static DkimSelector ToDkimSelector(EntityDkimSelector entityDkimSelector)
        {
            string selector = entityDkimSelector.Selector;

            List<DkimRecord> dkimRecords = entityDkimSelector.Records?.Select(ToDkimRecord).ToList();

            List <DkimMessage> dkimMessages = entityDkimSelector.Records?
                .SelectMany(_ => _?.Messages ?? new List<EntityMessage>())
                .Select(ToDkimMessage)
                .ToList();

            return new DkimSelector(selector, entityDkimSelector.CName, dkimRecords, dkimMessages);
        }

        private static DkimMessage ToDkimMessage(EntityMessage entityMessage)
        {
            return new DkimMessage(entityMessage.Name, entityMessage.MessageType, entityMessage.Text, entityMessage.MarkDown);
        }

        private static DkimRecord ToDkimRecord(EntityDkimRecord entityDkimRecord)
        {
            return new DkimRecord(entityDkimRecord.DnsRecord.Record);
        }
    }
}