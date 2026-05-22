using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateCustomerValidator : BaseValidator<CreateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(MaxEmailLength);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
        }
    }
}
