using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateCouponValidator : BaseValidator<CreateCouponDto>
    {
        public CreateCouponValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Type).NotEmpty().Must(t => t == "percent_off" || t == "amount_off")
                .WithMessage("Type must be 'percent_off' or 'amount_off'.");
            RuleFor(x => x.PercentOff).InclusiveBetween(1, 100)
                .When(x => x.Type == "percent_off")
                .WithMessage("PercentOff must be between 1 and 100.");
            RuleFor(x => x.AmountOff).GreaterThan(0)
                .When(x => x.Type == "amount_off")
                .WithMessage("AmountOff must be greater than 0.");
            RuleFor(x => x.Currency).NotEmpty()
                .When(x => x.Type == "amount_off")
                .WithMessage("Currency is required for amount_off coupons.");
            RuleFor(x => x.Duration).NotEmpty().Must(d => d == "once" || d == "repeating" || d == "forever")
                .WithMessage("Duration must be 'once', 'repeating', or 'forever'.");
            RuleFor(x => x.DurationInMonths).InclusiveBetween(1, 36)
                .When(x => x.Duration == "repeating")
                .WithMessage("DurationInMonths must be between 1 and 36 for repeating duration.");
            RuleFor(x => x.RedeemBy).GreaterThan(DateTime.UtcNow)
                .When(x => x.RedeemBy.HasValue)
                .WithMessage("RedeemBy must be a future date.");
        }
    }
}
