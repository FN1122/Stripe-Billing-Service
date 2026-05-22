using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateUsageRecordValidator : BaseValidator<CreateUsageRecordDto>
    {
        public CreateUsageRecordValidator()
        {
            RuleFor(x => x.SubscriptionId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Action).NotEmpty().Must(a => a == "increment" || a == "set")
                .WithMessage("Action must be 'increment' or 'set'.");
            RuleFor(x => x.IdempotencyKey).MaximumLength(200).When(x => x.IdempotencyKey != null);
        }
    }

    public class CreateMeterEventValidator : BaseValidator<CreateMeterEventDto>
    {
        public CreateMeterEventValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.EventName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }
}
