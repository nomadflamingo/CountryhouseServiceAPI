using System.ComponentModel.DataAnnotations;

namespace CountryhouseService.API.Helpers
{
    public sealed class LaterThanTodayOrTodayAttribute : ValidationAttribute
    {
        public LaterThanTodayOrTodayAttribute() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;
            if (value is DateTime dateTime)
            {
                if (dateTime < DateTime.Today)
                    return new ValidationResult("Date cannot be earlier than today's date", new string[] { validationContext.DisplayName });
                else
                    return ValidationResult.Success;
            }
            else
                return ValidationResult.Success;
        }
    }
}
