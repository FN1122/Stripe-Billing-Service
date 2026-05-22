using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class LoginRequestValidator : BaseValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(MaxEmailLength);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(MinPasswordLength);
        }
    }
}
