using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateSubscriptionValidator : BaseValidator<CreateSubscriptionDto>
    {
        public CreateSubscriptionValidator()
        {
            RuleFor(x => x.PlanId).NotEmpty();
            RuleFor(x => x).Must(x => x.CustomerId.HasValue || !string.IsNullOrEmpty(x.ExternalReferenceId))
                .WithMessage("Either CustomerId or ExternalReferenceId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
