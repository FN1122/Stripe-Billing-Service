using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreatePlanValidator : BaseValidator<CreatePlanDto>
    {
        public CreatePlanValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
            RuleFor(x => x.Interval).Must(i => i is "month" or "year" or "week" or "day");
        }
    }
}
