using FluentValidation;

namespace Core.Validators
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        protected const int MaxNameLength = 200;
        protected const int MaxEmailLength = 256;
        protected const int MinPasswordLength = 8;
        protected const int MaxPasswordLength = 100;
        protected const int MaxUrlLength = 500;
        protected const int MaxDescriptionLength = 1000;
    }
}
