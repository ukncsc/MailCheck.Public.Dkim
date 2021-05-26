using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Dao;
using Microsoft.Extensions.Logging;

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
        private readonly IDkimApiConfig _config;
        private readonly ILogger<DkimService> _log;

        public DkimService(IMessagePublisher messagePublisher, IDkimApiDao dao, IDkimApiConfig config, ILogger<DkimService> log)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
            _log = log;
        }

        public async Task<EntityDkimEntityState> GetDkimForDomain(string requestDomain)
        {
            EntityDkimEntityState response = await _dao.GetDkimSelectors(requestDomain);

            if (response == null)
            {
                _log.LogInformation($"Dkim entity state does not exist for domain {requestDomain} - publishing DomainMissing");
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}
