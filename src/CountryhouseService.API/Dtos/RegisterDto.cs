using CountryhouseService.API.Defaults;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace CountryhouseService.API.Dtos
{
    [Serializable]
    public record RegisterDto(
        [Required]
        [DataType(DataType.EmailAddress)]
        string Email,

        [Required]
        [DataType(DataType.Password)]
        string Password,

        [Required]
        [DataType(DataType.Password)]
        string ConfirmPassword,

        [Required]
        [DataType(DataType.PhoneNumber)]
        string PhoneNumber,

        [Required]
        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string FirstName,

        [Required]
        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string LastName,

        int? AvatarId,

        [Required]
        [MaxLength(20)]
        string Role);
}
