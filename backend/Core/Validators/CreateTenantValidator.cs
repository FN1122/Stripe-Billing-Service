using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateTenantValidator : BaseValidator<CreateTenantDto>
    {
        public CreateTenantValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
            RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
            RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Plan).Must(p => p is "starter" or "standard" or "advanced");
        }
    }
}
