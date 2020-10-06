using System;
using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Dao;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Mapping;
using MailCheck.Dkim.Api.Service;
using MailCheck.Dkim.Contracts.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Operation = MailCheck.Common.Api.Authorisation.Service.Domain.Operation;

namespace MailCheck.Dkim.Api.Controllers
{
    [Route("api/dkim")]
    public class DkimController : Controller
    {
        private readonly IDkimApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IDkimApiConfig _config;
        private readonly IDkimService _dkimService;
        private readonly IEntityToApiMapper _dkimEntityToApiMapper;

        public DkimController(IDkimApiDao dao,
            IMessagePublisher messagePublisher,
            IDkimService dkimService,
            IDkimApiConfig config, 
            IEntityToApiMapper dkimEntityToApiMapper)
        {
            _dao = dao;
            _messagePublisher = messagePublisher;
            _config = config;
            _dkimEntityToApiMapper = dkimEntityToApiMapper;
            _dkimService = dkimService;
        }

        [HttpGet("{domain}/recheck")]
        [MailCheckAuthoriseResource(Operation.Update, ResourceType.Dkim, "domain")]
        public async Task<IActionResult> RecheckDkim(DkimInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            DkimRecordsExpired recordsExpired = new DkimRecordsExpired(request.Domain, Guid.NewGuid().ToString(), null);

            await _messagePublisher.Publish(recordsExpired, _config.SnsTopicArn);

            return new OkObjectResult("{}");
        }

        [HttpGet]
        [Route("domain/{domain}")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetDkim(DkimInfoRequest request)
        {
            EntityDkimEntityState dkimEntityState = await _dkimService.GetDkimForDomain(request.Domain);

            if (dkimEntityState == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No Dkim found for {request.Domain}",
                    ErrorStatus.Information));
            }

            DkimResponse response = _dkimEntityToApiMapper.ToDkimResponse(dkimEntityState);

            return new ObjectResult(response);
        }

        [HttpGet]
        [Route("history/{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.DkimHistory, "domain")]
        public async Task<IActionResult> GetDkimHistory(DkimInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }
           
            string history = await _dao.GetDkimHistory(request.Domain);

            if (history == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No Dkim History found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return Content(history, "application/json");
        }
    }
}
