using FluentValidation.TestHelper;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Validators;
using NUnit.Framework;

namespace MailCheck.Dkim.Api.Test
{
    [TestFixture]
    public class DkimApiTest
    {
        private DkimInfoRequestValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new DkimInfoRequestValidator(new DomainValidator());
        }

        [Test]
        public void ItShouldHaveAnErrorWhenDomainIsNull()
        {
            _validator.ShouldHaveValidationErrorFor(request => request.Domain, null as string);
        }

        [Test]
        public void ItShouldHaveAnErrorWhenDomainIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(request => request.Domain, string.Empty);
        }

        [Test]
        public void ItShouldHaveAnErrorWhenDomainIsInvalid()
        {
            _validator.ShouldHaveValidationErrorFor(request => request.Domain, "abc");
        }

        [Test]
        public void ItShouldNotHaveAnErrorWhenDomainsIsNotEmptyOrNull()
        {
            _validator.ShouldNotHaveValidationErrorFor(request => request.Domain,  "abc.com");
        }
    }
}
