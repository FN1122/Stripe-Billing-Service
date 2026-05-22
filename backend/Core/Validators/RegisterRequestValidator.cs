using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class RegisterRequestValidator : BaseValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(MaxEmailLength);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(MaxNameLength);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(MinPasswordLength).MaximumLength(MaxPasswordLength);
            RuleFor(x => x.Role).Must(r => Core.Constants.Roles.IsValid(r)).WithMessage("Invalid role.");
        }
    }
}
