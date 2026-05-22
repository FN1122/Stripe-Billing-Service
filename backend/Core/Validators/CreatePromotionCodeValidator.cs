using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreatePromotionCodeValidator : BaseValidator<CreatePromotionCodeDto>
    {
        public CreatePromotionCodeValidator()
        {
            RuleFor(x => x.CouponId).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50)
                .Matches("^[a-zA-Z0-9-]+$").WithMessage("Code must be alphanumeric with dashes only.");
            RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0)
                .When(x => x.MinimumAmount.HasValue);
        }
    }
}
