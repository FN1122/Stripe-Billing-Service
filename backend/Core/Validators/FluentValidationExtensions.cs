using Core.Constants;
using FluentValidation;
using FluentValidation.Results;

namespace Core.Validators
{
    public static class FluentValidationExtensions
    {
        public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T? instance, string ruleSet) where T : class
        {
            if (instance == null)
            {
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.UnauthorizedAction)
                };
                throw new ValidationException(failures);
            }

            var validationResult = await validator.ValidateAsync(instance, options => options.IncludeRuleSets(ruleSet));
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T? instance, string ruleSet, IDictionary<string, object> rootContextData) where T : class
        {
            if (instance == null)
            {
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.UnauthorizedAction)
                };
                throw new ValidationException(failures);
            }

            var context = new ValidationContext<T>(instance,
                new FluentValidation.Internal.PropertyChain(),
                new FluentValidation.Internal.RulesetValidatorSelector(new[] { ruleSet }));

            if (rootContextData != null)
            {
                foreach (var kvp in rootContextData)
                {
                    context.RootContextData[kvp.Key] = kvp.Value;
                }
            }

            var validationResult = await validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
