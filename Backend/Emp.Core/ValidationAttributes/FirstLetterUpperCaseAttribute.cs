using System.ComponentModel.DataAnnotations;

namespace Emp.Core.ValidationAttributes;

public class FirstLetterUpperCaseAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string? strValue = value as string;
        if (string.IsNullOrWhiteSpace(strValue))
            return ValidationResult.Success; // Null or empty strings are considered valid by this attribute

        string firstLetter = strValue[0].ToString();
        string firstLetterUppercased = strValue[0].ToString().ToUpper();
        if (!firstLetter.Equals(firstLetterUppercased))
        {
            // Provide a default error message, or allow it to be overridden via constructor
            return new ValidationResult(ErrorMessage ?? "First letter must be uppercase.");
        }

        return ValidationResult.Success;
    }
}
