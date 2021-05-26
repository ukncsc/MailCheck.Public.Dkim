using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Dao.Model;

namespace MailCheck.Dkim.Scheduler.Handler
{
    public class DkimSchedulerHandler : 
        IHandle<DkimEntityCreated>,
        IHandle<DomainDeleted>
    {
        private readonly IDkimSchedulerDao _dkimSchedulerDao;
        private readonly ILogger<DkimSchedulerHandler> _log;

        public DkimSchedulerHandler(IDkimSchedulerDao dkimSchedulerDao,
            ILogger<DkimSchedulerHandler> log)
        {
            _dkimSchedulerDao = dkimSchedulerDao;
            _log = log;
        }

        public async Task Handle(DkimEntityCreated message)
        {
            string domain = message.Id.ToLower();
            DkimSchedulerState state = await _dkimSchedulerDao.Get(domain);

            if (state == null)
            {
                state = new DkimSchedulerState(domain);

                await _dkimSchedulerDao.Save(state);

                _log.LogInformation($"{domain} added to DkimScheduler");
            }
            else
            {
                _log.LogInformation($"{domain} already exists in DkimScheduler");
            }
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dkimSchedulerDao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted schedule for DKIM entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"Schedule already deleted for DKIM entity with id: {domain}.");
            }
            
        }
    }
}
