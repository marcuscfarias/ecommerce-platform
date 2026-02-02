using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.Application.ApplicationRules;

public class NotFoundApplicationRule(object? entity, string resourceName) : IBusinessRule
{
    public bool IsMet() => entity is not null;
    public string Error => $"{resourceName} was not found.";
    public Exception CreateException(string errorMessage) => new ApplicationRulesValidationException(errorMessage);
}