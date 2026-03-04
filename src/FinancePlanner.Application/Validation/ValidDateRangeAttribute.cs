using System.ComponentModel.DataAnnotations;

namespace FinancePlanner.Application.Validation;

/// <summary>
/// Validates that StartDate is before EndDate
/// </summary>
public class ValidDateRangeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var startDateProperty = instance.GetType().GetProperty("StartDate");
        var endDateProperty = instance.GetType().GetProperty("EndDate");

        if (startDateProperty == null || endDateProperty == null)
        {
            return ValidationResult.Success;
        }

        var startDate = startDateProperty.GetValue(instance) as DateTimeOffset?;
        var endDate = endDateProperty.GetValue(instance) as DateTimeOffset?;

        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            return new ValidationResult("Start date must be before end date");
        }

        return ValidationResult.Success;
    }
}
