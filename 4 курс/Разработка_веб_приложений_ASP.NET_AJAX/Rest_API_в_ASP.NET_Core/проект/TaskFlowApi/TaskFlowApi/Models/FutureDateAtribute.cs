using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Models;

/// <summary>
/// Проверяет, что дата находится в будущем.
/// </summary>
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime && dateTime <= DateTime.UtcNow)
        {
            return new ValidationResult("DueDate must be in the future.");
        }

        return ValidationResult.Success;
    }
}