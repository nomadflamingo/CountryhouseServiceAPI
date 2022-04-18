using System.ComponentModel.DataAnnotations;

namespace CountryhouseService.API.Extensions
{
    /// <summary>
    /// Specifies the maximum amount of elements in a collection
    /// </summary>
    public sealed class MaxIntsAttribute : ValidationAttribute
    {
        private readonly int maxAmount;

        public MaxIntsAttribute(int maxAmount)
        {
            this.maxAmount = maxAmount;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;
            if (value is ICollection<int> enumerable)
            {
                if (enumerable.Count > maxAmount)
                    return new ValidationResult($"Collection cannot have more than {maxAmount} integers");
                else
                    return ValidationResult.Success;
            }
            else
                return new ValidationResult($"{nameof(MaxIntsAttribute)} can only be applied to a collection of integers");
        }
    }
}
