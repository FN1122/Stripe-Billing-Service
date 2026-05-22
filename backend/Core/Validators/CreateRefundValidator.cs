using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateRefundValidator : BaseValidator<CreateRefundDto>
    {
        public CreateRefundValidator()
        {
            RuleFor(x => x.TransactionId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0).When(x => x.Amount.HasValue);
            RuleFor(x => x.Reason).Must(r => r is "duplicate" or "fraudulent" or "requested_by_customer" or "other").When(x => !string.IsNullOrEmpty(x.Reason));
        }
    }
}
