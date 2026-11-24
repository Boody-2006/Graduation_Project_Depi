using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// Validates that a DateTime represents an age not less than the specified minimum.
    /// </summary>
    public sealed class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;

                if (age < _minimumAge)
                {
                    return new ValidationResult(ErrorMessage ?? $"Must be at least {_minimumAge} years old.");
                }
            }
            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validates that a DateTime represents an age not exceeding the specified maximum.
    /// </summary>
    public sealed class MaximumAgeAttribute : ValidationAttribute
    {
        private readonly int _maximumAge;

        public MaximumAgeAttribute(int maximumAge)
        {
            _maximumAge = maximumAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                if (dateOfBirth > DateTime.Today)
                {
                    return new ValidationResult(
                        ErrorMessage ?? "Date of birth cannot be in the future / تاريخ الميلاد لا يمكن أن يكون في المستقبل");
                }

                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;

                if (age > _maximumAge)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"Date of birth appears invalid (age over {_maximumAge}) / أدخل تاريخ ميلاد صحيح");
                }
            }
            return ValidationResult.Success;
        }
    }
}
