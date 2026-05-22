using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateApiKeyValidator : BaseValidator<CreateApiKeyDto>
    {
        public CreateApiKeyValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
            RuleFor(x => x.Environment).Must(e => e is "test" or "live");
            RuleFor(x => x.RateLimitPerMinute).GreaterThan(0).LessThanOrEqualTo(10000);
        }
    }
}
