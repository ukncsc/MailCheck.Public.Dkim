using FluentValidation;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Domain;

namespace MailCheck.Dkim.Api.Validators
{
    public class DkimInfoRequestValidator : AbstractValidator<DkimInfoRequest>
    {
        public DkimInfoRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domains must be be a valid domain");
        }
    }
}
