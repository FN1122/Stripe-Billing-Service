using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateCheckoutValidator : BaseValidator<CreateCheckoutDto>
    {
        public CreateCheckoutValidator()
        {
            RuleFor(x => x.LineItems).NotEmpty().WithMessage("At least one line item is required.");
            RuleForEach(x => x.LineItems).ChildRules(item =>
            {
                item.RuleFor(i => i.Name).NotEmpty();
                item.RuleFor(i => i.Amount).GreaterThan(0);
                item.RuleFor(i => i.Currency).NotEmpty().Length(3);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });
            RuleFor(x => x.SuccessUrl).NotEmpty().Must(u => Uri.TryCreate(u, UriKind.Absolute, out _));
            RuleFor(x => x.CancelUrl).NotEmpty().Must(u => Uri.TryCreate(u, UriKind.Absolute, out _));
            RuleFor(x => x.Mode).Must(m => m is "payment" or "subscription");
        }
    }
}
