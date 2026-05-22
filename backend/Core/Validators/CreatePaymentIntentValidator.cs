using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreatePaymentIntentValidator : BaseValidator<CreatePaymentIntentDto>
    {
        public CreatePaymentIntentValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }
}
