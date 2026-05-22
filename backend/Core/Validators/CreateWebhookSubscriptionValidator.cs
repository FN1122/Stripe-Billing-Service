using Core.Dtos.Requests;
using FluentValidation;

namespace Core.Validators
{
    public class CreateWebhookSubscriptionValidator : BaseValidator<CreateWebhookSubscriptionDto>
    {
        public CreateWebhookSubscriptionValidator()
        {
            RuleFor(x => x.WebhookUrl).NotEmpty().MaximumLength(MaxUrlLength).Must(u => Uri.TryCreate(u, UriKind.Absolute, out _));
            RuleFor(x => x.Events).NotEmpty().WithMessage("At least one event type is required.");
        }
    }
}
