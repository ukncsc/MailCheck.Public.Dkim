using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Dao;

namespace MailCheck.Dkim.Api.Service
{
    public interface IDkimService
    {
        Task<EntityDkimEntityState> GetDkimForDomain(string requestDomain);
    }

    public class DkimService : IDkimService
    {
        private readonly IDkimApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IClock _clock;
        private readonly IDkimApiConfig _config;

        private const int DaysBeforeBeingConsideredStale = 2;

        public DkimService(IMessagePublisher messagePublisher, IDkimApiDao dao, IDkimApiConfig config, IClock clock)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
            _clock = clock;
        }

        public async Task<EntityDkimEntityState> GetDkimForDomain(string requestDomain)
        {
            EntityDkimEntityState response = await _dao.GetDkimSelectors(requestDomain);

            if (response?.RecordsLastUpdated == null || response.RecordsLastUpdated.Value.AddDays(DaysBeforeBeingConsideredStale) <= _clock.GetDateTimeUtc())
            {
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}
