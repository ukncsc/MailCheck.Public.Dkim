using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.External;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.EntityHistory.Dao;
using MailCheck.Dkim.EntityHistory.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dkim.EntityHistory.Entity
{
    public class DkimEntityHistory : IHandle<DkimEvaluationUpdated>
    {
        private readonly IDkimHistoryEntityDao _dao;
        private readonly ILogger<DkimEntityHistory> _log;

        public DkimEntityHistory(
            ILogger<DkimEntityHistory> log,
            IDkimHistoryEntityDao dao)
        {
            _dao = dao;
            _log = log;
        }

        public async Task Handle(DkimEvaluationUpdated message)
        {
            string domain = message.Id.ToLower();

            DkimHistoryEntityState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new DkimHistoryEntityState(domain);
                _log.LogInformation("Created DkimEntityHistory for {Id}.", domain);
            }

            if (state.UpdateHistory(message.ToDkimHistoryRecord()))
            {
                await _dao.Save(state);
                _log.LogInformation("Dkim History updated for {Id}.", domain);
            }
        }
    }
}