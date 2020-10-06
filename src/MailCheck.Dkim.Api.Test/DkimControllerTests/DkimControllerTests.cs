using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Controllers;
using MailCheck.Dkim.Api.Dao;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Mapping;
using MailCheck.Dkim.Api.Service;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace MailCheck.Dkim.Api.Test.DkimControllerTests
{
    [TestFixture]
    public class DkimControllerTests
    {
        private DkimController _sut;
        private IDkimService _dkimService;
        private IEntityToApiMapper _entityToApiMapper;

        [SetUp]
        public void SetUp()
        {
            _dkimService = A.Fake<IDkimService>();
            _entityToApiMapper = A.Fake<IEntityToApiMapper>();
            _sut = new DkimController(A.Fake<IDkimApiDao>(), A.Fake<IMessagePublisher>(),
                _dkimService, A.Fake<IDkimApiConfig>(), _entityToApiMapper);
        }

        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoDkimState()
        {
            A.CallTo(() => _dkimService.GetDkimForDomain(A<string>._))
                .Returns(Task.FromResult<EntityDkimEntityState>(null));

            IActionResult response = await _sut.GetDkim(new DkimInfoRequest { Domain = "ncsc.gov.uk" });

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheDkimStateExists()
        {
            EntityDkimEntityState state = new EntityDkimEntityState(null, EntityDkimState.Created, 0, DateTime.MinValue, null, null, null);

            A.CallTo(() => _dkimService.GetDkimForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            DkimResponse dkimResponse = new DkimResponse("", State.Created, null, null);

            A.CallTo(() => _entityToApiMapper.ToDkimResponse(state))
                .Returns(dkimResponse);

            ObjectResult response = (ObjectResult)await _sut.GetDkim(new DkimInfoRequest { Domain = "ncsc.gov.uk" });

            Assert.AreSame(dkimResponse, response.Value);
        }

        [Test]
        public void AllMethodsHaveAuthorisation()
        {
            IEnumerable<MethodInfo> controllerMethods = _sut.GetType().GetMethods().Where(x => x.DeclaringType == typeof(DkimController));

            foreach (MethodInfo methodInfo in controllerMethods)
            {
                Assert.That(methodInfo.CustomAttributes.Any(x => x.AttributeType == typeof(MailCheckAuthoriseResourceAttribute) || x.AttributeType == typeof(MailCheckAuthoriseRoleAttribute)));
            }
        }
    }
}
